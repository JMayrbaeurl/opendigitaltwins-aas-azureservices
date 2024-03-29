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
    public partial class SubmodelElement : Referable, IEquatable<SubmodelElement>
    { 
        /// <summary>
        /// Gets or Sets Kind
        /// </summary>

        [DataMember(Name="kind")]
        public ModelingKind Kind { get; set; }

        /// <summary>
        /// Gets or Sets SemanticId
        /// </summary>

        [DataMember(Name="semanticId")]
        public Reference SemanticId { get; set; }

        /// <summary>
        /// Gets or Sets SupplementalSemanticIds
        /// </summary>

        [DataMember(Name="supplementalSemanticIds")]
        public List<Reference> SupplementalSemanticIds { get; set; }

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
        /// Gets or Sets EmbeddedDataSpecifications
        /// </summary>

        [DataMember(Name="embeddedDataSpecifications")]
        public List<EmbeddedDataSpecification> EmbeddedDataSpecifications { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SubmodelElement {\n");
            sb.Append("  Kind: ").Append(Kind).Append("\n");
            sb.Append("  SemanticId: ").Append(SemanticId).Append("\n");
            sb.Append("  SupplementalSemanticIds: ").Append(SupplementalSemanticIds).Append("\n");
            sb.Append("  Qualifiers: ").Append(Qualifiers).Append("\n");
            sb.Append("  ModelType: ").Append(ModelType).Append("\n");
            sb.Append("  EmbeddedDataSpecifications: ").Append(EmbeddedDataSpecifications).Append("\n");
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
            return obj.GetType() == GetType() && Equals((SubmodelElement)obj);
        }

        /// <summary>
        /// Returns true if SubmodelElement instances are equal
        /// </summary>
        /// <param name="other">Instance of SubmodelElement to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SubmodelElement other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Kind == other.Kind ||
                    Kind != null &&
                    Kind.Equals(other.Kind)
                ) && 
                (
                    SemanticId == other.SemanticId ||
                    SemanticId != null &&
                    SemanticId.Equals(other.SemanticId)
                ) && 
                (
                    SupplementalSemanticIds == other.SupplementalSemanticIds ||
                    SupplementalSemanticIds != null &&
                    SupplementalSemanticIds.SequenceEqual(other.SupplementalSemanticIds)
                ) && 
                (
                    Qualifiers == other.Qualifiers ||
                    Qualifiers != null &&
                    Qualifiers.SequenceEqual(other.Qualifiers)
                ) && 
                (
                    ModelType == other.ModelType ||
                    ModelType != null &&
                    ModelType.Equals(other.ModelType)
                ) && 
                (
                    EmbeddedDataSpecifications == other.EmbeddedDataSpecifications ||
                    EmbeddedDataSpecifications != null &&
                    EmbeddedDataSpecifications.SequenceEqual(other.EmbeddedDataSpecifications)
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
                    if (Kind != null)
                    hashCode = hashCode * 59 + Kind.GetHashCode();
                    if (SemanticId != null)
                    hashCode = hashCode * 59 + SemanticId.GetHashCode();
                    if (SupplementalSemanticIds != null)
                    hashCode = hashCode * 59 + SupplementalSemanticIds.GetHashCode();
                    if (Qualifiers != null)
                    hashCode = hashCode * 59 + Qualifiers.GetHashCode();
                    if (ModelType != null)
                    hashCode = hashCode * 59 + ModelType.GetHashCode();
                    if (EmbeddedDataSpecifications != null)
                    hashCode = hashCode * 59 + EmbeddedDataSpecifications.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(SubmodelElement left, SubmodelElement right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SubmodelElement left, SubmodelElement right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}
