/*
 * DotAAS Part 2 | HTTP/REST | Entire API Collection
 *
 * The entire API collection as part of Details of the Asset Administration Shell Part 2
 *
 * OpenAPI spec version: V1.0RC03
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AAS.API.Models
{ 
        /// <summary>
        /// Gets or Sets ExecutionState
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum ExecutionState
        {
            /// <summary>
            /// Enum InitiatedEnum for Initiated
            /// </summary>
            [EnumMember(Value = "Initiated")]
            InitiatedEnum = 0,
            /// <summary>
            /// Enum RunningEnum for Running
            /// </summary>
            [EnumMember(Value = "Running")]
            RunningEnum = 1,
            /// <summary>
            /// Enum CompletedEnum for Completed
            /// </summary>
            [EnumMember(Value = "Completed")]
            CompletedEnum = 2,
            /// <summary>
            /// Enum CanceledEnum for Canceled
            /// </summary>
            [EnumMember(Value = "Canceled")]
            CanceledEnum = 3,
            /// <summary>
            /// Enum FailedEnum for Failed
            /// </summary>
            [EnumMember(Value = "Failed")]
            FailedEnum = 4,
            /// <summary>
            /// Enum TimeoutEnum for Timeout
            /// </summary>
            [EnumMember(Value = "Timeout")]
            TimeoutEnum = 5        }
}