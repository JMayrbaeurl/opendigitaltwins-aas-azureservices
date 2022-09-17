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
    /// 
    /// </summary>
    [DataContract]
    public partial class Qualifiable : IEquatable<Qualifiable>
    { 
        /// <summary>
        /// Gets or Sets Qualifiers
        /// </summary>

        [DataMember(Name="qualifiers")]
        public List<Qualifier> Qualifiers { get; set; }

        /// <summary>
        /// Gets or Sets ModelType
        /// </summary>
        [Required]

        [DataMember(Name="modelType")]
        public ModelType ModelType { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Qualifiable {\n");
            sb.Append("  Qualifiers: ").Append(Qualifiers).Append("\n");
            sb.Append("  ModelType: ").Append(ModelType).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Qualifiable)obj);
        }

        /// <summary>
        /// Returns true if Qualifiable instances are equal
        /// </summary>
        /// <param name="other">Instance of Qualifiable to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Qualifiable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Qualifiers == other.Qualifiers ||
                    Qualifiers != null &&
                    Qualifiers.SequenceEqual(other.Qualifiers)
                ) && 
                (
                    ModelType == other.ModelType ||
                    ModelType != null &&
                    ModelType.Equals(other.ModelType)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                    if (Qualifiers != null)
                    hashCode = hashCode * 59 + Qualifiers.GetHashCode();
                    if (ModelType != null)
                    hashCode = hashCode * 59 + ModelType.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(Qualifiable left, Qualifiable right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Qualifiable left, Qualifiable right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
