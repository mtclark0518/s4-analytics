﻿using Dapper;
using Microsoft.Extensions.Options;
using MoreLinq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace S4Analytics.Models
{
    public class LookupKeyAndName
    {
        public int key;
        public string name;
    }

    public class CrashReportingRepository
    {
        private const int MIN_DAYS_BACK = 15;
        private readonly string _connStr;

        public CrashReportingRepository(
            IOptions<ServerOptions> serverOptions)
        {
            _connStr = serverOptions.Value.FlatConnStr;
        }

        public IEnumerable<LookupKeyAndName> GetGeographyLookups()
        {
            var queryText = @"SELECT id AS key, cnty_nm || ' County, FL' AS name
                FROM dim_geography
                WHERE city_cd = 0
                AND cnty_cd <> 68
                UNION ALL
                SELECT id AS key, city_nm || ', FL' AS name
                FROM dim_geography
                WHERE city_cd <> 0
                AND cnty_cd <> 68
                ORDER BY name";
            IEnumerable<LookupKeyAndName> results;
            using (var conn = new OracleConnection(_connStr))
            {
                results = conn.Query<LookupKeyAndName>(queryText, new { });
            }
            return results;
        }

        public IEnumerable<LookupKeyAndName> GetAgencyLookups()
        {
            var queryText = @"SELECT id AS key, agncy_nm AS name
                FROM dim_agncy
                ORDER BY agncy_nm";
            IEnumerable<LookupKeyAndName> results;
            using (var conn = new OracleConnection(_connStr))
            {
                results = conn.Query<LookupKeyAndName>(queryText, new { });
            }
            return results;
        }

        private class PreparedQuery
        {
            public string queryText;
            public Dictionary<string, object> queryParameters;
            public DynamicParameters DynamicParams
            {
                get
                {
                    var dynamicParams = new DynamicParameters();
                    dynamicParams.AddDict(queryParameters);
                    return dynamicParams;
                }
            }
            public PreparedQuery(string queryText, Dictionary<string, object> queryParameters)
            {
                this.queryText = queryText;
                this.queryParameters = queryParameters;
            }
        }

        public ReportOverTime<int> GetCrashCountsByYear(CrashesOverTimeQuery query)
        {
            string[] monthNames = new[] { "","Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" };

            // find the last day of the last full month that ended at least MIN_DAYS_BACK days ago
            var nDaysAgo = DateTime.Now.Subtract(new TimeSpan(MIN_DAYS_BACK, 0, 0, 0));
            var maxDate = new DateTime(nDaysAgo.Year, nDaysAgo.Month, 1).Subtract(new TimeSpan(1, 0, 0, 0));
            var minDate = new DateTime(maxDate.Year - 4, 1, 1); // include 4 calendar years prior to maxDate
            bool isFullYear = maxDate.Month == 12;

            int series1StartMonth = 1;
            int series1EndMonth = maxDate.Month;
            string series1Format = series1StartMonth == series1EndMonth
                ? "{0}" // single-month series
                : "{0} - {1}"; // multiple-month series
            string series1Label = string.Format(series1Format, monthNames[series1StartMonth], monthNames[series1EndMonth]);

            int? series2StartMonth;
            int? series2EndMonth;
            string series2Label = "";
            if (!isFullYear)
            {
                series2StartMonth = maxDate.Month + 1;
                series2EndMonth = 12;
                string series2Format = series2StartMonth == series2EndMonth
                    ? "{0}" // single-month series
                    : "{0} - {1}"; // multiple-month series
                series2Label = string.Format(series2Format, monthNames[(int)series2StartMonth], monthNames[(int)series2EndMonth]);
            }

            var preparedQuery = PrepareQuery(query);

            var queryText = $@"WITH grouped_cts AS (
                -- count matching crashes, grouped by year and month
                SELECT /*+ RESULT_CACHE */
                    crash_yr,
                    crash_mm,
                    COUNT(*) ct
                FROM crash_evt
                WHERE key_crash_dt BETWEEN TRUNC(:minDate) AND TRUNC(:maxDate)
                AND ( {preparedQuery.queryText} )
                GROUP BY crash_yr, crash_mm
            )
            SELECT /*+ RESULT_CACHE */ -- sum previous counts, grouped by series and year
                CASE WHEN crash_mm <= :series1EndMonth THEN 1 ELSE 2 END AS seq,
                CASE WHEN crash_mm <= :series1EndMonth THEN :series1Label ELSE :series2Label END AS series,
                CAST(crash_yr AS VARCHAR2(4)) AS category,
                SUM(ct) AS ct
            FROM grouped_cts
            GROUP BY
                CASE WHEN crash_mm <= :series1EndMonth THEN 1 ELSE 2 END,
                CASE WHEN crash_mm <= :series1EndMonth THEN :series1Label ELSE :series2Label END,
                crash_yr
            ORDER BY
                CASE WHEN crash_mm <= :series1EndMonth THEN 1 ELSE 2 END,
                crash_yr";

            var dynamicParams = preparedQuery.DynamicParams;
            dynamicParams.Add(new
            {
                series1EndMonth,
                series1Label,
                series2Label,
                maxDate,
                minDate
            });

            var report = new ReportOverTime<int>() { maxDate = maxDate };
            using (var conn = new OracleConnection(_connStr))
            {
                var results = conn.Query(queryText, dynamicParams);
                report.categories = results.DistinctBy(r => r.CATEGORY).Select(r => (string)(r.CATEGORY));
                var seriesNames = results.DistinctBy(r => r.SERIES).Select(r => (string)(r.SERIES));
                var series = new List<ReportSeries<int>>();
                foreach (var seriesName in seriesNames)
                {
                    series.Add(new ReportSeries<int>()
                    {
                        name = seriesName,
                        data = results.Where(r => r.SERIES == seriesName).Select(r => (int)r.CT)
                    });
                }
                report.series = series;
            }
            return report;
        }

        public ReportOverTime<int> GetCrashCountsByMonth(int year, CrashesOverTimeQuery query)
        {
            // find the last day of the last full month that ended at least MIN_DAYS_BACK days ago
            var nDaysAgo = DateTime.Now.Subtract(new TimeSpan(MIN_DAYS_BACK, 0, 0, 0));
            var maxDate = new DateTime(nDaysAgo.Year, nDaysAgo.Month, 1).Subtract(new TimeSpan(1, 0, 0, 0));

            if (year < maxDate.Year)
            {
                maxDate = new DateTime(year, 12, 31);
            }

            var preparedQuery = PrepareQuery(query);

            var queryText = $@"WITH grouped_cts AS (
                -- count matching crashes, grouped by year and month
                SELECT /*+ RESULT_CACHE */
                    crash_yr,
                    crash_mm,
                    crash_mo,
                    COUNT(*) ct
                FROM crash_evt
                WHERE crash_yr IN (:year, :year - 1)
                AND key_crash_dt < TRUNC(:maxDate + 1)
                AND ( {preparedQuery.queryText} )
                GROUP BY crash_yr, crash_mm, crash_mo
            )
            SELECT /*+ RESULT_CACHE */ -- sum previous counts, grouped by series and month
                CAST(crash_yr AS VARCHAR2(4)) AS series,
                crash_mo AS category,
                SUM(ct) AS ct
            FROM grouped_cts
            GROUP BY crash_yr, crash_mm, crash_mo
            ORDER BY crash_yr, crash_mm";

            var dynamicParams = preparedQuery.DynamicParams;
            dynamicParams.Add(new
            {
                maxDate,
                maxDate.Year
            });

            var report = new ReportOverTime<int>() { maxDate = maxDate };
            using (var conn = new OracleConnection(_connStr))
            {
                var results = conn.Query(queryText, dynamicParams);
                report.categories = results.DistinctBy(r => r.CATEGORY).Select(r => (string)(r.CATEGORY));
                var seriesNames = results.DistinctBy(r => r.SERIES).Select(r => (string)(r.SERIES));
                var series = new List<ReportSeries<int>>();
                foreach (var seriesName in seriesNames)
                {
                    series.Add(new ReportSeries<int>()
                    {
                        name = seriesName,
                        data = results.Where(r => r.SERIES == seriesName).Select(r => (int)r.CT)
                    });
                }
                report.series = series;
            }
            return report;
        }

        public ReportOverTime<int?> GetCrashCountsByDay(int year, bool alignByWeek, CrashesOverTimeQuery query)
        {
            // find the date MIN_DAYS_BACK days ago
            DateTime maxDate = DateTime.Now.Subtract(new TimeSpan(MIN_DAYS_BACK, 0, 0, 0));

            if (year < maxDate.Year)
            {
                maxDate = new DateTime(year, 12, 31);
            }

            string innerQueryText;

            if (alignByWeek)
            {
                innerQueryText = @"SELECT
                    :year AS yr,
                    dd1.evt_dt,
                    :year - 1 AS prev_yr,
                    dd2.evt_dt AS prev_yr_dt
                FROM dim_date dd1
                FULL OUTER JOIN dim_date dd2
                    ON dd2.evt_dt = dd1.prev_yr_dt_align_day_of_wk -- align by day of week
                WHERE ( dd1.evt_yr = :year OR dd2.evt_yr = :year - 1 )
                ORDER BY dd2.evt_dt, dd1.evt_dt";
            }
            else
            {
                innerQueryText = @"SELECT
                    :year AS yr,
                    dd1.evt_dt,
                    :year - 1 AS prev_yr,
                    dd2.evt_dt AS prev_yr_dt
                FROM dim_date dd1
                FULL OUTER JOIN dim_date dd2
                    ON dd2.evt_dt = dd1.prev_yr_dt_align_day_of_mo -- align by day of month
                WHERE ( dd1.evt_yr = :year OR dd2.evt_yr = :year - 1 )
                AND (
                    dd2.evt_dt IS NULL -- INCLUDE null record if current year has feb 29
                    OR dd2.evt_mm <> 2 OR dd2.evt_dd <> 29  -- EXCLUDE feb 29 prior year
                )
                ORDER BY dd1.evt_dt";
            }

            var preparedQuery = PrepareQuery(query);

            var queryText = $@"WITH
            aligned_dts AS (
                SELECT /*+ RESULT_CACHE */
                    ROWNUM AS seq, yr, evt_dt, prev_yr, prev_yr_dt
                FROM ( {innerQueryText} )
            ),
            crash_cts AS (
                SELECT /*+ RESULT_CACHE */
                    key_crash_dt, COUNT(*) AS ct
                FROM crash_evt ce
                WHERE crash_yr BETWEEN :year - 1 AND :year
                AND ( {preparedQuery.queryText} )
                GROUP BY key_crash_dt
            )
            SELECT /*+ RESULT_CACHE */
                TO_CHAR(yr) AS series, seq, evt_dt,
                CASE
                    WHEN evt_dt IS NULL OR evt_dt >= TRUNC(:maxDate + 1) THEN NULL
                    ELSE NVL(ct, 0)
                END AS ct
            FROM (
                SELECT
                    ad.yr, ad.seq, ad.evt_dt, cts.ct
                FROM aligned_dts ad
                LEFT OUTER JOIN crash_cts cts
                    ON cts.key_crash_dt = ad.evt_dt
                UNION ALL
                SELECT
                    ad.prev_yr AS yr, ad.seq, ad.prev_yr_dt AS evt_dt, cts.ct
                FROM aligned_dts ad
                LEFT OUTER JOIN crash_cts cts
                    ON cts.key_crash_dt = ad.prev_yr_dt
            ) res
            ORDER BY yr, seq";

            var dynamicParams = preparedQuery.DynamicParams;
            dynamicParams.Add(new
            {
                maxDate,
                maxDate.Year,
                isLeapYear = DateTime.IsLeapYear(maxDate.Year) ? 1 : 0
            });

            var report = new ReportOverTime<int?>() { maxDate = maxDate };
            using (var conn = new OracleConnection(_connStr))
            {
                var results = conn.Query(queryText, dynamicParams);
                var seriesNames = results.DistinctBy(r => r.SERIES).Select(r => (string)(r.SERIES));
                var series = new List<ReportSeries<int?>>();
                foreach (var seriesName in seriesNames)
                {
                    var seriesData = results.Where(r => r.SERIES == seriesName);
                    series.Add(new ReportSeries<int?>()
                    {
                        name = seriesName,
                        data = seriesData.Select(r => (int?)r.CT)
                    });
                }
                report.series = series;
            }
            return report;
        }

        private PreparedQuery PrepareQuery(CrashesOverTimeQuery query)
        {
            // initialize where clause and query parameter collections
            var whereClauses = new List<string>();
            var queryParameters = new Dictionary<string, object>();

            // get predicate methods
            var predicateMethods = GetPredicateMethods(query);

            // generate where clause and query parameters for each valid filter
            predicateMethods.ForEach(generatePredicate => {
                (var whereClause, var parameters) = generatePredicate.Invoke();
                if (whereClause != null)
                {
                    whereClauses.Add(whereClause);
                    if (parameters != null)
                    {
                        queryParameters.AddFields(parameters);
                    }
                }
            });

            // join where clauses
            if (whereClauses.Count == 0)
            {
                // prevent the query from breaking if there are no where clauses
                whereClauses.Add("1=1");
            }
            var queryText = "(" + string.Join(")\r\nAND (", whereClauses) + ")";

            return new PreparedQuery(queryText, queryParameters);
        }

        private List<Func<(string, object)>> GetPredicateMethods(CrashesOverTimeQuery query)
        {
            Func<(string, object)>[] predicateMethods =
            {
                () => GenerateGeographyPredicate(query.geographyId),
                () => GenerateReportingAgencyPredicate(query.reportingAgencyId),
                () => GenerateSeverityPredicate(query.severity),
                () => GenerateAlcoholDrugPredicate(query.impairment),
                () => GenerateBikePedPredicate(query.bikePedRelated),
                () => GenerateCmvPredicate(query.cmvRelated),
                () => GenerateCodeablePredicate(query.codeable),
                () => GenerateFormTypePredicate(query.formType)
            };
            return predicateMethods.ToList();
        }

        private (string whereClause, object parameters) GenerateGeographyPredicate(int? geographyId)
        {
            // test for valid filter
            if (geographyId == null)
            {
                return (null, null);
            }

            var isCounty = geographyId % 100 == 0;

            // define where clause
            var whereClause = isCounty
                ? @"cnty_cd = :geographyId / 100"
                : @"key_geography = :geographyId";

            // define oracle parameters
            var parameters = new { geographyId };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateReportingAgencyPredicate(int? reportingAgencyId)
        {
            // test for valid filter
            if (reportingAgencyId == null)
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"(:isFhpTroop = 0 AND key_rptg_agncy = :reportingAgencyId)
                OR (:isFhpTroop = 1 AND key_rptg_agncy = 1 AND key_rptg_unit = :reportingAgencyId)";

            // define oracle parameters
            var parameters = new
            {
                isFhpTroop = reportingAgencyId > 1 && reportingAgencyId <= 14 ? 1 : 0,
                reportingAgencyId
            };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateSeverityPredicate(CrashesOverTimeSeverity severity)
        {
            // test for valid filter
            if (severity == null || !(severity.propertyDamageOnly || severity.injury || severity.fatality))
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"(:pdo = 1 AND key_crash_sev = 30)
                OR (:injury = 1 AND key_crash_sev = 31)
                OR (:fatality = 1 AND key_crash_sev = 32)";

            // define oracle parameters
            var parameters = new {
                pdo = severity.propertyDamageOnly ? 1 : 0,
                injury = severity.injury ? 1 : 0,
                fatality = severity.fatality ? 1 : 0
            };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateAlcoholDrugPredicate(CrashesOverTimeImpairment impairment)
        {
            // test for valid filter
            if (impairment == null || !(impairment.drugRelated || impairment.alcoholRelated))
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"(:drugRelated = 1 AND is_drug_rel = 'Y')
                OR (:alcoholRelated = 1 AND is_alc_rel = 'Y')";

            // define oracle parameters
            var parameters = new {
                drugRelated = impairment.drugRelated ? 1 : 0,
                alcoholRelated = impairment.alcoholRelated ? 1 : 0
            };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateBikePedPredicate(CrashesOverTimeBikePedRelated bikePedRelated)
        {
            // test for valid filter
            if (bikePedRelated == null || !(bikePedRelated.bikeRelated || bikePedRelated.pedRelated))
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"(:bikeRelated = 1 AND (bike_cnt > 0 OR key_first_he = 11))
                OR (:pedRelated = 1 AND (ped_cnt > 0 OR key_first_he = 10))";

            // define oracle parameters
            var parameters = new {
                bikeRelated = bikePedRelated.bikeRelated ? 1 : 0,
                pedRelated = bikePedRelated.pedRelated ? 1 : 0
            };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateCmvPredicate(bool? cmvRelated)
        {
            // test for valid filter
            if (cmvRelated == null || cmvRelated != true)
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"comm_veh_cnt > 0";

            // define oracle parameters
            var parameters = new { };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateCodeablePredicate(bool? codeable)
        {
            // test for valid filter
            if (codeable == null || codeable != true)
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"codeable = 'Y'";

            // define oracle parameters
            var parameters = new { };

            return (whereClause, parameters);
        }

        private (string whereClause, object parameters) GenerateFormTypePredicate(CrashesOverTimeFormType formTypes)
        {
            // test for valid filter
            if (formTypes == null || !(formTypes.longForm || formTypes.shortForm))
            {
                return (null, null);
            }

            // define where clause
            var whereClause = @"(:longForm = 1 AND form_type_cd = 'L')
                OR (:shortForm = 1 AND form_type_cd = 'S')";

            // define oracle parameters
            var parameters = new {
                longForm = formTypes.longForm ? 1 : 0,
                shortForm = formTypes.shortForm ? 1 : 0
            };

            return (whereClause, parameters);
        }
    }
}