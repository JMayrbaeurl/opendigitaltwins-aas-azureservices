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
        /// Gets or Sets KeyElements
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum KeyElements
        {
            /// <summary>
            /// Enum AssetAdministrationShellEnum for AssetAdministrationShell
            /// </summary>
            [EnumMember(Value = "AssetAdministrationShell")]
            AssetAdministrationShellEnum = 0,
            /// <summary>
            /// Enum AccessPermissionRuleEnum for AccessPermissionRule
            /// </summary>
            [EnumMember(Value = "AccessPermissionRule")]
            AccessPermissionRuleEnum = 1,
            /// <summary>
            /// Enum ConceptDescriptionEnum for ConceptDescription
            /// </summary>
            [EnumMember(Value = "ConceptDescription")]
            ConceptDescriptionEnum = 2,
            /// <summary>
            /// Enum SubmodelEnum for Submodel
            /// </summary>
            [EnumMember(Value = "Submodel")]
            SubmodelEnum = 3,
            /// <summary>
            /// Enum AnnotatedRelationshipElementEnum for AnnotatedRelationshipElement
            /// </summary>
            [EnumMember(Value = "AnnotatedRelationshipElement")]
            AnnotatedRelationshipElementEnum = 4,
            /// <summary>
            /// Enum BasicEventEnum for BasicEvent
            /// </summary>
            [EnumMember(Value = "BasicEvent")]
            BasicEventEnum = 5,
            /// <summary>
            /// Enum BlobEnum for Blob
            /// </summary>
            [EnumMember(Value = "Blob")]
            BlobEnum = 6,
            /// <summary>
            /// Enum CapabilityEnum for Capability
            /// </summary>
            [EnumMember(Value = "Capability")]
            CapabilityEnum = 7,
            /// <summary>
            /// Enum DataElementEnum for DataElement
            /// </summary>
            [EnumMember(Value = "DataElement")]
            DataElementEnum = 8,
            /// <summary>
            /// Enum FileEnum for File
            /// </summary>
            [EnumMember(Value = "File")]
            FileEnum = 9,
            /// <summary>
            /// Enum EntityEnum for Entity
            /// </summary>
            [EnumMember(Value = "Entity")]
            EntityEnum = 10,
            /// <summary>
            /// Enum EventEnum for Event
            /// </summary>
            [EnumMember(Value = "Event")]
            EventEnum = 11,
            /// <summary>
            /// Enum MultiLanguagePropertyEnum for MultiLanguageProperty
            /// </summary>
            [EnumMember(Value = "MultiLanguageProperty")]
            MultiLanguagePropertyEnum = 12,
            /// <summary>
            /// Enum OperationEnum for Operation
            /// </summary>
            [EnumMember(Value = "Operation")]
            OperationEnum = 13,
            /// <summary>
            /// Enum PropertyEnum for Property
            /// </summary>
            [EnumMember(Value = "Property")]
            PropertyEnum = 14,
            /// <summary>
            /// Enum RangeEnum for Range
            /// </summary>
            [EnumMember(Value = "Range")]
            RangeEnum = 15,
            /// <summary>
            /// Enum ReferenceElementEnum for ReferenceElement
            /// </summary>
            [EnumMember(Value = "ReferenceElement")]
            ReferenceElementEnum = 16,
            /// <summary>
            /// Enum RelationshipElementEnum for RelationshipElement
            /// </summary>
            [EnumMember(Value = "RelationshipElement")]
            RelationshipElementEnum = 17,
            /// <summary>
            /// Enum SubmodelElementEnum for SubmodelElement
            /// </summary>
            [EnumMember(Value = "SubmodelElement")]
            SubmodelElementEnum = 18,
            /// <summary>
            /// Enum SubmodelElementListEnum for SubmodelElementList
            /// </summary>
            [EnumMember(Value = "SubmodelElementList")]
            SubmodelElementListEnum = 19,
            /// <summary>
            /// Enum SubmodelElementStructEnum for SubmodelElementStruct
            /// </summary>
            [EnumMember(Value = "SubmodelElementStruct")]
            SubmodelElementStructEnum = 20,
            /// <summary>
            /// Enum ViewEnum for View
            /// </summary>
            [EnumMember(Value = "View")]
            ViewEnum = 21,
            /// <summary>
            /// Enum FragmentReferenceEnum for FragmentReference
            /// </summary>
            [EnumMember(Value = "FragmentReference")]
            FragmentReferenceEnum = 22        }
}
