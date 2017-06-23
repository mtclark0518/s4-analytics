INSERT INTO geocode_result (
  OBJECTID,
  HSMV_RPT_NBR,
  SCORE_NBR,
  LOCATION_TYPE_CD,
  MATCH_STATUS_CD,
  MATCH_RESULT_CD,
  STD_ADDR_TX,
  MATCH_ADDR_TX,
  ADDR_USED_CD,
  CITY_USED_CD,
  BYPASS_USED_CD,
  CUSTOM_GEOCODER_USED_CD,
  UNMATCHED_DESC_CD,
  MAP_POINT_X,
  MAP_POINT_Y,
  MAP_POINT_SRID,
  CENTER_LINE_X,
  CENTER_LINE_Y,
  CENTER_LINE_SRID,
  CRASH_SEG_ID,
  NEAREST_INTRSECT_ID,
  NEAREST_INTRSECT_OFFSET_FT,
  NEAREST_INTRSECT_OFFSET_DIR,
  REF_INTRSECT_ID,
  REF_INTRSECT_OFFSET_FT,
  REF_INTRSECT_OFFSET_DIR,
  UPDT_DIR_TRAVEL,
  LAST_UPDT_USER_ID,
  LAST_UPDT_DT,
  SYM_ANGLE,
  SOURCE_FORMAT_CD,
  GEOCODE_ENGINE_CD,
  METHOD_USED_CD,
  ON_NETWORK,
  DOT_ON_SYS,
  KEY_GEOGRAPHY,
  CITY_CD,
  CNTY_CD,
  MAPPED,
  BATCH_NBR,
  SHAPE,
  AUTHOR,
  ST_NM,
  INTRSECT_ST_NM,
  ST_NBR,
  FAIL_MATCH_RSN_CD,
  RE_GEOLOCATED_FROM_VER,
  AM_INTRSECT_ID,
  AM_INTRSECT_OFFSET_FT,
  AM_INTRSECT_OFFSET_DIR,
  REL_TO_NETWORK)
SELECT *
FROM v_geocode_result_for_sdo@lime_navteq_2015q1 gr
WHERE EXISTS (
  SELECT 1
  FROM s4_warehouse_ng.fact_crash_evt fce
  WHERE fce.hsmv_rpt_nbr = gr.hsmv_rpt_nbr
);

INSERT INTO s_citation (
  OBJECTID,
  CITATION_NBR,
  SHAPE,
  MPO_BND_ID)
SELECT *
FROM v_st_citation_for_sdo@lime_navteq_2015q1 sc
WHERE EXISTS (
  SELECT 1
  FROM s4_warehouse_ng.citation ci
  WHERE ci.citation_nbr = sc.citation_nbr
);

INSERT INTO st
SELECT
  OBJECTID,
  LINK_ID,
  ST_NAME,
  FEAT_ID,
  ST_LANGCD,
  NUM_STNMES,
  ST_NM_PREF,
  ST_TYP_BEF,
  ST_NM_BASE,
  ST_NM_SUFF,
  ST_TYP_AFT,
  ST_TYP_ATT,
  ADDR_TYPE,
  L_REFADDR,
  L_NREFADDR,
  L_ADDRSCH,
  L_ADDRFORM,
  R_REFADDR,
  R_NREFADDR,
  R_ADDRSCH,
  R_ADDRFORM,
  REF_IN_ID,
  NREF_IN_ID,
  N_SHAPEPNT,
  FUNC_CLASS,
  SPEED_CAT,
  FR_SPD_LIM,
  TO_SPD_LIM,
  TO_LANES,
  FROM_LANES,
  ENH_GEOM,
  LANE_CAT,
  DIVIDER,
  DIR_TRAVEL,
  L_AREA_ID,
  R_AREA_ID,
  L_POSTCODE,
  R_POSTCODE,
  L_NUMZONES,
  R_NUMZONES,
  NUM_AD_RNG,
  AR_AUTO,
  AR_BUS,
  AR_TAXIS,
  AR_CARPOOL,
  AR_PEDEST,
  AR_TRUCKS,
  AR_TRAFF,
  AR_DELIV,
  AR_EMERVEH,
  AR_MOTOR,
  PAVED,
  "PRIVATE",
  FRONTAGE,
  BRIDGE,
  TUNNEL,
  RAMP,
  TOLLWAY,
  POIACCESS,
  CONTRACC,
  ROUNDABOUT,
  INTERINTER,
  UNDEFTRAFF,
  FERRY_TYPE,
  MULTIDIGIT,
  MAXATTR,
  SPECTRFIG,
  INDESCRIB,
  MANOEUVRE,
  DIVIDERLEG,
  INPROCDATA,
  FULL_GEOM,
  URBAN,
  ROUTE_TYPE,
  DIRONSIGN,
  EXPLICATBL,
  NAMEONRDSN,
  POSTALNAME,
  STALENAME,
  VANITYNAME,
  JUNCTIONNM,
  EXITNAME,
  SCENIC_RT,
  SCENIC_NM,
  FOURWHLDR,
  COVERIND,
  PLOT_ROAD,
  REVERSIBLE,
  EXPR_LANE,
  CARPOOLRD,
  PHYS_LANES,
  VER_TRANS,
  PUB_ACCESS,
  LOW_MBLTY,
  PRIORITYRD,
  SPD_LM_SRC,
  EXPAND_INC,
  TRANS_AREA,
  DESCRIPT,
  FGDLAQDATE,
  NULL AS SHAPE,
  CITY_CD,
  CNTY_CD
FROM st@lime_navteq_2015q1;

INSERT INTO intrsect
SELECT *
FROM intrsect@lime_navteq_2015q1;

INSERT INTO intrsect_node
SELECT *
FROM intrsect_node@lime_navteq_2015q1;

INSERT INTO st_ext
SELECT *
FROM st_ext@lime_navteq_2015q1;

COMMIT;
