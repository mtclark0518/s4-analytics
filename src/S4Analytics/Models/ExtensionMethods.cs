﻿using Dapper;
using Lib.Identity.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace S4Analytics.Models
{
    public static class ExtensionMethods
    {
        public static Stream ToStream(this string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Extension method that works like AddDynamicParams() but
        /// does not have the limitation/bug that parameter names are
        /// missing from the ParameterNames collection.
        /// </summary>
        /// <param name="dynamicParams">DynamicParameters instance</param>
        /// <param name="item">Parameters object whose fields should be added to the DynamicParameters instance</param>
        public static void Add(this DynamicParameters dynamicParams, object item)
        {
            // add a parameter for each field in the template object
            foreach (var prop in item.GetType().GetProperties())
            {
                dynamicParams.Add(prop.Name, prop.GetValue(item));
            }
        }

        /// <summary>
        /// Add Dictionary contents to DynamicParameters instance.
        /// </summary>
        /// <param name="dynamicParams">DynamicParameters instance.</param>
        /// <param name="parameters">Dictionary whose values should be added to the DynamicParameters instance.</param>
        public static void AddDict(this DynamicParameters dynamicParams, Dictionary<string, object> parameters)
        {
            // add a parameter for each field in the template object
            foreach (var key in parameters.Keys)
            {
                dynamicParams.Add(key, parameters[key]);
            }
        }

        /// <summary>
        /// Add all fields of an object to a dictionary.
        /// </summary>
        /// <param name="dict">Dictionary instance.</param>
        /// <param name="item">Item whose fields should be added to the Dictionary instance.</param>
        public static void AddFields(this Dictionary<string, object> dict, object item)
        {
            // add a parameter for each field in the template object
            foreach (var prop in item.GetType().GetProperties())
            {
                dict.Add(prop.Name, prop.GetValue(item));
            }
        }

        /// <summary>
        /// Serialize object to JSON.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>JSON representation of object.</returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Serialize object to pretty JSON.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Pretty JSON representation of object.</returns>
        public static string ToPrettyJson(this object obj)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            return JsonConvert.SerializeObject(obj, serializerSettings);
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            session.SetString(key, JsonConvert.SerializeObject(value, serializerSettings));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var value = session.GetString(key);
            return value == null
                ? default(T)
                : JsonConvert.DeserializeObject<T>(value, serializerSettings);
        }

        // there is probably a better place for the methods below

        public static bool IsUserManager(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.UserManager.ToLowerInvariant());
        }

        public static bool IsGlobalAdmin(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.GlobalAdmin.ToLowerInvariant());
        }

        public static bool IsEditor(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.Editor.ToLowerInvariant());
        }

        public static bool IsPbcatEditor(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.PbcatEditor.ToLowerInvariant());
        }

        public static bool IsGuest(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.Guest.ToLowerInvariant());
        }

        public static bool IsHSMVAdmin(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.HSMVAdmin.ToLowerInvariant());
        }

        public static bool IsFDOTAdmin(this S4IdentityUser<S4UserProfile> user)
        {
            return user.Roles.Any(role => role.RoleName == RoleNames.FDOTAdmin.ToLowerInvariant());
        }


    }
}
