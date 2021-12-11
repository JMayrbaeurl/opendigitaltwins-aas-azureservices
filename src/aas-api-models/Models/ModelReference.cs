/*
 * DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository
 *
 * An exemplary interface combination for the use case of an Asset Administration Shell Repository
 *
 * OpenAPI spec version: Final-Draft
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
    public partial class ModelReference : Reference, IEquatable<ModelReference>
    { 
        /// <summary>
        /// Gets or Sets ReferredSemanticId
        /// </summary>

        [DataMember(Name="referredSemanticId")]
        public Reference ReferredSemanticId { get; set; }

        /// <summary>
        /// Gets or Sets Keys
        /// </summary>
        [Required]

        [DataMember(Name="keys")]
        public List<Key> Keys { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ModelReference {\n");
            sb.Append("  ReferredSemanticId: ").Append(ReferredSemanticId).Append("\n");
            sb.Append("  Keys: ").Append(Keys).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public  new string ToJson()
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
            return obj.GetType() == GetType() && Equals((ModelReference)obj);
        }

        /// <summary>
        /// Returns true if ModelReference instances are equal
        /// </summary>
        /// <param name="other">Instance of ModelReference to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ModelReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    ReferredSemanticId == other.ReferredSemanticId ||
                    ReferredSemanticId != null &&
                    ReferredSemanticId.Equals(other.ReferredSemanticId)
                ) && 
                (
                    Keys == other.Keys ||
                    Keys != null &&
                    Keys.SequenceEqual(other.Keys)
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
                    if (ReferredSemanticId != null)
                    hashCode = hashCode * 59 + ReferredSemanticId.GetHashCode();
                    if (Keys != null)
                    hashCode = hashCode * 59 + Keys.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(ModelReference left, ModelReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelReference left, ModelReference right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
