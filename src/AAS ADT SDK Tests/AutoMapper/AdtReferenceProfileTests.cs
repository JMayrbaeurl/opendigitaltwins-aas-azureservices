using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using AAS.ADT.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace AAS.ADT.Tests.AutoMapper
{
    [TestClass]
    public class AdtReferenceProfileTests
    {
        private IMapper? _objectUnderTest;
        private AdtReference _fullAdtReference;
        private AdtReference _minimalAdtReference;
        private Reference _fullReference;
        private Reference _minimalReference;

        [TestInitialize]
        public void Setup()
        {

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AdtReferenceProfile());
                cfg.DisableConstructorMapping();
                cfg.AllowNullCollections = true;
            });
            configuration.AssertConfigurationIsValid();
            _objectUnderTest = configuration.CreateMapper();

            _minimalReference = new Reference(ReferenceTypes.ModelReference, new List<Key>(), null);
            _minimalAdtReference = new AdtReference
            {
                Key1 = null,
                Key2 = null,
                Key3 = null,
                Key4 = null,
                Key5 = null,
                Key6 = null,
                Key7 = null,
                Key8 = null,
                Type = "ModelReference"
            };

            _fullReference = new Reference(ReferenceTypes.GlobalReference, new List<Key>()
            {
                new Key(KeyTypes.GlobalReference, "value1"),
                new Key(KeyTypes.File, "value2"),
                new Key(KeyTypes.Property, "value3"),
                new Key(KeyTypes.AnnotatedRelationshipElement, "value4"),
                new Key(KeyTypes.Capability, "value5"),
                new Key(KeyTypes.ConceptDescription, "value6"),
                new Key(KeyTypes.Entity, "value7"),
                new Key(KeyTypes.Referable, "value8")
            });
            _fullAdtReference = new AdtReference
            {
                Key1 = new AdtKey
                {
                    Type = "GlobalReference",
                    Value = "value1"
                },
                Key2 = new AdtKey
                {
                    Type = "File",
                    Value = "value2"
                },
                Key3 = new AdtKey
                {
                    Type = "Property",
                    Value = "value3"
                },
                Key4 = new AdtKey
                {
                    Type = "AnnotatedRelationshipElement",
                    Value = "value4"
                },
                Key5 = new AdtKey
                {
                    Type = "Capability",
                    Value = "value5"
                },
                Key6 = new AdtKey
                {
                    Type = "ConceptDescription",
                    Value = "value6"
                },
                Key7 = new AdtKey
                {
                    Type = "Entity",
                    Value = "value7"
                },
                Key8 = new AdtKey
                {
                    Type = "Referable",
                    Value = "value8"
                },
                Type = "GlobalReference"
            };


        }

        [TestMethod]
        public void Map_minimal_AdtReference_to_Reference()
        {
            var actual = _objectUnderTest.Map<Reference>(_minimalAdtReference);
            actual.Should().BeEquivalentTo(_minimalReference);
        }

        [TestMethod]
        public void Map_full_AdtReference_to_Reference()
        {
            var actual = _objectUnderTest.Map<Reference>(_fullAdtReference);
            actual.Should().BeEquivalentTo(_fullReference);
        }



    }
}
