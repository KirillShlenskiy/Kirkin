using System;

using Kirkin.Mapping;
using Kirkin.Mapping.Engine.Compilers;
using Kirkin.Reflection;

using NUnit.Framework;

namespace Kirkin.Tests.Mapping
{
    public class MapperTests
    {
        [Test]
        public void MemberMapEquality()
        {
            MapperBuilder<Dummy, LowercaseDummy> builder1 = new MapperBuilder<Dummy, LowercaseDummy> {
                MemberNameComparer = StringComparer.OrdinalIgnoreCase
            };

            MapperBuilder<Dummy, LowercaseDummy> builder2 = new MapperBuilder<Dummy, LowercaseDummy> {
                MemberNameComparer = StringComparer.OrdinalIgnoreCase,
                NullableBehaviour = NullableBehaviour.DefaultMapsToNull
            };

            Assert.True(
                new MemberMappingCollection<Dummy, LowercaseDummy>(builder1.ProduceValidMemberMappings()).Equals(
                    new MemberMappingCollection<Dummy, LowercaseDummy>(builder2.ProduceValidMemberMappings())),
                "Maps are not equal."
            );

            builder1.TargetMember(dst => dst.id).MapTo(src => src.ID + 1);

            Assert.False(
                new MemberMappingCollection<Dummy, LowercaseDummy>(builder1.ProduceValidMemberMappings()).Equals(
                    new MemberMappingCollection<Dummy, LowercaseDummy>(builder2.ProduceValidMemberMappings())),
                "Maps are equal."
            );
        }

        [Test]
        public void CustomMemberMappingExpression()
        {
            var source = new Dummy { ID = 5, Value = "Test" };
            var target = new LowercaseDummy();

            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                MemberNameComparer = StringComparer.OrdinalIgnoreCase
            };

            builder.TargetMember(dst => dst.id).MapTo(src => src.ID + 1);
            builder.TargetMember("value").MapTo(_ => "TEZD");

            builder.BuildMapper().Map(source, target);

            Assert.AreEqual(source.ID + 1, target.id);
            Assert.AreEqual("TEZD", target.value);
        }

        [Test]
        public void CustomMemberMappingDelegate()
        {
            var source = new Dummy { ID = 5, Value = "Test" };
            var target = new LowercaseDummy();

            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                MemberNameComparer = StringComparer.OrdinalIgnoreCase
            };

            builder.TargetMember(dst => dst.id).MapWithDelegate(src => src.ID + 1);
            builder.TargetMember(dst => dst.value).MapWithDelegate(_ => "TEZD");

            builder.BuildMapper().Map(source, target);

            Assert.AreEqual(source.ID + 1, target.id);
            Assert.AreEqual("TEZD", target.value);
        }

        [Test]
        public void CustomUnmappedMemberMapping()
        {
            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            Assert.AreEqual(0, builder.ProduceValidMemberMappings().Length);

            builder.TargetMember(d => d.id).MapTo(d => d.ID);

            Assert.AreEqual(1, builder.ProduceValidMemberMappings().Length);

            var target = builder.BuildMapper().Map(new Dummy { ID = 123 });

            Assert.AreEqual(123, target.id);
        }

        [Test]
        public void ExecuteEmptyMapping()
        {
            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            Assert.AreEqual(0, builder.ProduceValidMemberMappings().Length);
            builder.BuildMapper().Map(new Dummy());
        }

        [Test]
        public void IgnoreMember()
        {
            var source = new Dummy { ID = 5, Value = "Test" };

            var builder = new MapperBuilder<Dummy, Dummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            builder.TargetMember(d => d.Value).Ignore();

            var target = builder.BuildMapper().Map(source);

            Assert.AreEqual(5, target.ID);
            Assert.Null(target.Value);
        }

        [Test]
        public void IgnoreUnmappedMemberDoesNotThrow()
        {
            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            builder.TargetMember(m => m.id).Ignore();
            builder.ValidateMapping();

            builder = new MapperBuilder<Dummy, LowercaseDummy>{
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            builder.TargetMember(m => m.id).MapTo(builder.SourceMember(m => m.ID).Member.Name);
            builder.TargetMember(m => m.value).Ignore();
            builder.ValidateMapping();
        }

        [Test]
        public void ManualMapping()
        {
            var memberMappings = new MapperBuilder<Dummy, LowercaseDummy> { MemberNameComparer = StringComparer.OrdinalIgnoreCase }
                .ProduceValidMemberMappings();

            var engine = new MappingCompiler<Dummy, LowercaseDummy>();
            var mappingDelegate = engine.CompileMapping(memberMappings);
            var source = new Dummy { ID = 5, Value = "Test" };
            var target = new LowercaseDummy();

            mappingDelegate(source, target);

            Assert.AreEqual(source.ID, target.id);
            Assert.AreEqual(source.Value, target.value);
        }

        [Test]
        public void CompileMapBenchmark()
        {
            var mappings = new MapperBuilder<ExtendedDummy, Dummy> { AllowUnmappedSourceMembers = true }
                .ProduceValidMemberMappings();

            var compiler = new MappingCompiler<ExtendedDummy, Dummy>();

            for (int i = 0; i < 1000; i++) {
                compiler.CompileMapping(mappings);
            }
        }

        [Test]
        public void CompileMapBenchmarkCached()
        {
            var mappings = new MapperBuilder<ExtendedDummy, Dummy>() { AllowUnmappedSourceMembers = true }
                .ProduceValidMemberMappings();

            var compiler = new CachedMappingCompiler<ExtendedDummy, Dummy>();

            for (int i = 0; i < 100000; i++) {
                compiler.CompileMapping(mappings);
            }
        }

        [Test]
        public void MapSameType()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = Mapper.MapStrict<Dummy, Dummy>(dummy1);

            Assert.AreEqual(dummy1.ID, dummy2.ID);
            Assert.AreEqual(dummy1.Value, dummy2.Value);
        }

        [Test]
        public void MapDerivedType()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new ExtendedDummy();

            Mapper.Map(dummy1, dummy2);

            Assert.AreEqual(dummy1.ID, dummy2.ID);
            Assert.AreEqual(dummy1.Value, dummy2.Value);
        }

        [Test]
        public void MapDerivedTypeDynamic()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new ExtendedDummy();

            Mapper.MapStrict<Dummy, Dummy>(dummy1, dummy2);

            Assert.AreEqual(dummy1.ID, dummy2.ID);
            Assert.AreEqual(dummy1.Value, dummy2.Value);
        }

        [Test]
        public void MapperStrictByDefault()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new ExtendedDummy();

            Assert.Throws<MappingException>(() => Mapper.MapStrict(dummy1, dummy2));
        }

        [Test]
        public void MapperCaseSensitiveByDefault()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new LowercaseDummy();

            Assert.Throws<MappingException>(() => Mapper.MapStrict(dummy1, dummy2));
        }

        [Test]
        public void MapDifferentTypes()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };

            var dummy2 = new MapperBuilder<Dummy, LowercaseDummy> { MemberNameComparer = StringComparer.OrdinalIgnoreCase }
                .BuildMapper()
                .Map(dummy1);

            Assert.AreEqual(dummy1.ID, dummy2.id);
            Assert.AreEqual(dummy1.Value, dummy2.value);
        }

        [Test]
        public void ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => Mapper.MapStrict<object, object>(null, null));
        }

        [Test]
        public void NullableToNonNullableMapping()
        {
            var dummy1 = new NullableDummy();
            var dummy2 = new Dummy { ID = 1 };

            Mapper.MapStrict(dummy1, dummy2);
            Assert.AreEqual(0, dummy2.ID);
        }

        [Test]
        public void NonNullableToNullableMapping()
        {
            var dummy1 = new Dummy();
            var dummy2 = new NullableDummy { ID = 1 };

            Mapper.MapStrict(dummy1, dummy2);
            Assert.Null(dummy2.ID);
        }

        [Test]
        public void NonNullableToNullableMappingDoNotSubstituteDefault()
        {
            var dummy1 = new Dummy();
            var dummy2 = new NullableDummy { ID = 1 };

            var builder = new MapperBuilder<Dummy, NullableDummy> {
                NullableBehaviour = NullableBehaviour.AssignDefaultAsIs,
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            builder.BuildMapper().Map(dummy1, dummy2);
            Assert.AreEqual(0, dummy2.ID);
        }

        [Test]
        public void NonNullableToNullableMappingError()
        {
            var dummy1 = new Dummy();
            var dummy2 = new NullableDummy { ID = 1 };
            var mapper = new MapperBuilder<Dummy, NullableDummy> { NullableBehaviour = NullableBehaviour.Error }.BuildMapper();

            Assert.Throws<MappingException>(() => mapper.Map(dummy1, dummy2));
        }

        [Test]
        public void MappingSucceeds()
        {
            var dummy = new Dummy { ID = 1, Value = "Test" };

            var extendedDummy = new MapperBuilder<Dummy, ExtendedDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            }
            .BuildMapper()
            .Map(dummy);

            Assert.AreEqual(dummy.ID, extendedDummy.ID);
            Assert.AreEqual(dummy.Value, extendedDummy.Value);

            extendedDummy = Mapper.MapRelaxed<Dummy, ExtendedDummy>(dummy);

            Assert.AreEqual(dummy.ID, extendedDummy.ID);
            Assert.AreEqual(dummy.Value, extendedDummy.Value);

            new MapperBuilder<Dummy, ExtendedDummy> { AllowUnmappedTargetMembers = true }.BuildMapper().Map(dummy, extendedDummy);
            Mapper.MapAllSourceMembers(dummy, extendedDummy);
            new MapperBuilder<ExtendedDummy, Dummy> { AllowUnmappedSourceMembers = true }.BuildMapper().Map(extendedDummy, dummy);
            Mapper.MapAllTargetMembers(extendedDummy, dummy);
        }

        [Test]
        public void MappingThrows()
        {
            var dummy = new Dummy();
            var extendedDummy = new ExtendedDummy();

            Assert.Throws<MappingException>(() =>
                new MapperBuilder<Dummy, ExtendedDummy>().ValidateMapping()
            );

            Assert.Throws<MappingException>(() => Mapper.MapStrict(new Dummy(), new ExtendedDummy()));

            Assert.Throws<MappingException>(() =>
                new MapperBuilder<Dummy, ExtendedDummy> { AllowUnmappedSourceMembers = true }.ValidateMapping()
            );

            Assert.Throws<MappingException>(() => Mapper.MapAllTargetMembers(new Dummy(), new ExtendedDummy()));

            Assert.Throws<MappingException>(() =>
                new MapperBuilder<ExtendedDummy, Dummy> { AllowUnmappedTargetMembers = true }.ValidateMapping()
            );

            Assert.Throws<MappingException>(() => Mapper.MapAllSourceMembers(new ExtendedDummy(), new Dummy()));
        }

        [Test]
        public void ConvertibleSupport()
        {
            var dummy = new Dummy { ID = 5 };
            var target = Mapper.MapStrict(dummy, new ConvertibleDummy());

            Assert.AreEqual(5m, target.ID);
        }

        [Test]
        public void NullableConvertibleSupport()
        {
            var dummy = new Dummy { ID = 5 };
            var target = Mapper.MapStrict(dummy, new NullableConvertibleDummy());

            Assert.AreEqual(5m, target.ID);

            dummy.ID = 0;

            Mapper.MapStrict(dummy, target);

            Assert.False(target.ID.HasValue);

            new MapperBuilder<Dummy, NullableConvertibleDummy> { NullableBehaviour = NullableBehaviour.AssignDefaultAsIs }
                .BuildMapper()
                .Map(dummy, target);

            Assert.AreEqual(0, target.ID);
        }

        [Test]
        public void StringToEnum()
        {
            Assert.AreEqual(ValueEnum.Test, Mapper.MapStrict(new Dummy { Value = "TEST" }, new EnumDummy()).Value);
            Assert.AreEqual(ValueEnum.None, Mapper.MapStrict(new Dummy { Value = null }, new EnumDummy()).Value);
            Assert.Throws<ArgumentException>(() => Mapper.MapStrict(new Dummy { Value = "" }, new EnumDummy()));
        }
            
        [Test]
        public void StringToNullableEnum()
        {
            Assert.AreEqual(ValueEnum.Test, Mapper.MapStrict(new Dummy { Value = "TEST" }, new NullableEnumDummy()).Value);
            Assert.Null(Mapper.MapStrict(new Dummy { Value = null }, new NullableEnumDummy()).Value);
            Assert.Throws<ArgumentException>(() => Mapper.MapStrict(new Dummy { Value = "" }, new NullableEnumDummy()));
        }

        [Test]
        public void EnumToString()
        {
            Assert.AreEqual("Test", Mapper.MapStrict(new EnumDummy { Value = ValueEnum.Test }, new Dummy()).Value);
            Assert.AreEqual("None", Mapper.MapStrict(new EnumDummy(), new Dummy()).Value);
        }

        [Test]
        public void NullableEnumToString()
        {
            Assert.AreEqual("Test", Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.Test }, new Dummy()).Value);
            Assert.AreEqual("None", Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.None }, new Dummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableEnumDummy { Value = null }, new Dummy()).Value);
        }

        [Test]
        public void IntToEnum()
        {
            Assert.AreEqual(ValueEnum.Test, Mapper.MapStrict(new IntValueDummy { Value = 1 }, new EnumDummy()).Value);
            Assert.AreEqual(ValueEnum.None, Mapper.MapStrict(new IntValueDummy { Value = 0 }, new EnumDummy()).Value);
        }

        [Test]
        public void IntToNullableEnum()
        {
            Assert.AreEqual(ValueEnum.Test, Mapper.MapStrict(new IntValueDummy { Value = 1 }, new NullableEnumDummy()).Value);
            Assert.Null(Mapper.MapStrict(new IntValueDummy { Value = 0 }, new NullableEnumDummy()).Value);
        }

        [Test]
        public void NullableIntToEnum()
        {
            Assert.AreEqual(ValueEnum.Test, Mapper.MapStrict(new NullableIntValueDummy { Value = 1 }, new EnumDummy()).Value);
            Assert.AreEqual(ValueEnum.None, Mapper.MapStrict(new NullableIntValueDummy { Value = 0 }, new EnumDummy()).Value);
            Assert.AreEqual(ValueEnum.None, Mapper.MapStrict(new NullableIntValueDummy { Value = null }, new EnumDummy()).Value);
        }

        [Test]
        public void NullableIntToNullableEnum()
        {
            Assert.AreEqual(ValueEnum.Test, Mapper.MapStrict(new NullableIntValueDummy { Value = 1 }, new NullableEnumDummy()).Value);
            Assert.AreEqual(ValueEnum.None, Mapper.MapStrict(new NullableIntValueDummy { Value = 0 }, new NullableEnumDummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableIntValueDummy { Value = null }, new NullableEnumDummy()).Value);
        }

        [Test]
        public void EnumToInt()
        {
            Assert.AreEqual(1, Mapper.MapStrict(new EnumDummy { Value = ValueEnum.Test }, new IntValueDummy()).Value);
            Assert.AreEqual(0, Mapper.MapStrict(new EnumDummy { Value = ValueEnum.None }, new IntValueDummy()).Value);
        }

        [Test]
        public void EnumToNullableInt()
        {
            Assert.AreEqual(1, Mapper.MapStrict(new EnumDummy { Value = ValueEnum.Test }, new NullableIntValueDummy()).Value);
            Assert.Null(Mapper.MapStrict(new EnumDummy { Value = ValueEnum.None }, new NullableIntValueDummy()).Value);
        }

        [Test]
        public void NullableEnumToInt()
        {
            Assert.AreEqual(1, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.Test }, new IntValueDummy()).Value);
            Assert.AreEqual(0, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.None }, new IntValueDummy()).Value);
            Assert.AreEqual(0, Mapper.MapStrict(new NullableEnumDummy { Value = null }, new IntValueDummy()).Value);
        }

        [Test]
        public void NullableEnumToNullableInt()
        {
            Assert.AreEqual(1, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.Test }, new NullableIntValueDummy()).Value);
            Assert.AreEqual(0, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.None }, new NullableIntValueDummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableEnumDummy { Value = null }, new NullableIntValueDummy()).Value);
        }

        [Test]
        public void IntToString()
        {
            Assert.AreEqual("0", Mapper.MapStrict(new IntValueDummy { Value = 0 }, new Dummy()).Value);
            Assert.AreEqual("0", Mapper.MapStrict(new NullableIntValueDummy { Value = 0 }, new Dummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableIntValueDummy { Value = null }, new Dummy()).Value);
        }

        [Test]
        public void NullStringToNullableInt()
        {
            // Strict mapping: even though we could technically execute it
            // for the case where Value = null, all other cases would fail,
            // so going from string -> Nullable<T> is prohibited.
            Assert.Throws<InvalidOperationException>(() => Mapper.MapStrict(new Dummy { Value = null }, new NullableIntValueDummy()));
        }

        [Test]
        public void StructMapping()
        {
            var source = new Size { Width = 2, Height = 4 };
            var target = Mapper.MapStrict(source, new Size());

            Assert.AreEqual(2, target.Width);
            Assert.AreEqual(4, target.Height);
        }

        [Test]
        public void SourceMemberIgnore()
        {
            var builder = new MapperBuilder<ExtendedDummy, Dummy>();
            var extendedDummy = new ExtendedDummy { ID = 123, Value = "Blah", Guid = Guid.NewGuid() };

            Assert.Throws<MappingException>(() => builder.BuildMapper().Map(extendedDummy));

            builder.SourceMember(d => d.Guid).Ignore();

            var dummy = builder.BuildMapper().Map(extendedDummy);

            Assert.AreEqual(123, dummy.ID);
            Assert.AreEqual("Blah", dummy.Value);
        }

        [Test]
        public void IgnorePreviouslyMappedMemberCausesValidationError()
        {
            var builder = new MapperBuilder<Dummy, Dummy>();

            builder.TargetMember(d => d.ID).Ignore();

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember(d => d.ID).Reset();

            builder.ValidateMapping();

            Assert.AreEqual(2, builder.ProduceValidMemberMappings().Length);

            builder.TargetMember(d => d.ID).Ignore(ignoreMatchingSource: true);

            Assert.AreEqual(1, builder.ProduceValidMemberMappings().Length);

            builder.ValidateMapping();
        }

        [Test]
        public void TotalRemapWithStrictValidationByExpression()
        {
            var builder = new MapperBuilder<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember(d => d.IDz).MapTo(d => d.ID);

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember(d => d.Valuez).MapTo(d => d.Value);

            builder.ValidateMapping();
        }

        [Test]
        public void TotalRemapWithStrictValidationByName()
        {
            var builder = new MapperBuilder<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember("IDz").MapTo("ID");

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember("Valuez").MapTo("Value");

            builder.ValidateMapping();
        }

        [Test]
        public void MemberAccessHonoursConfigsNameComparer()
        {
            var builder = new MapperBuilder<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => builder.SourceMember("id"));
            Assert.Throws<MappingException>(() => builder.TargetMember("idz"));
            Assert.Throws<MappingException>(() => builder.TargetMember("IDz").MapTo("id"));

            builder.MemberNameComparer = StringComparer.OrdinalIgnoreCase;

            builder.SourceMember("id");
            builder.TargetMember("idz");
            builder.TargetMember("IDz").MapTo("id");
        }

        [Test]
        public void CustomMappingsHonourConfigsNullableBehaviour()
        {
            var builder = new MapperBuilder<Dummy, NullableDummy>();

            builder.NullableBehaviour = NullableBehaviour.Error;

            Assert.Throws<MappingException>(() => builder.BuildMapper().Map(new Dummy())); // Value auto-mapped.

            builder.TargetMember(d => d.ID).MapTo("ID");

            Assert.Throws<MappingException>(() => builder.BuildMapper().Map(new Dummy())); // Value manually mapped but nullable conversion fails.

            builder.NullableBehaviour = NullableBehaviour.DefaultMapsToNull;

            var result = builder.BuildMapper().Map(new Dummy { ID = 0 }, new NullableDummy { ID = 123 });

            Assert.Null(result.ID);
        }

        [Test]
        public void MapperFromTypeMapping()
        {
            Dummy d1 = new Dummy { ID = 123, Value = "Zzz" };

            Dummy d2 = Mapper.Builder
                .FromMembers(PropertyMember.PropertyListMembers(PropertyList<Dummy>.Default.Without(d => d.Value)))
                .ToMembers(PropertyMember.PropertyListMembers(PropertyList<Dummy>.Default.Without(d => d.Value)))
                .BuildMapper()
                .Map(d1);

            Assert.AreEqual(123, d2.ID);
            Assert.Null(d2.Value);
        }

        struct Size
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        sealed class ExtendedDummy : Dummy
        {
            public Guid Guid { get; set; }
        }

        sealed class ExtendedDummyReadOnlyGuid : Dummy
        {
            public Guid Guid { get; private set; }
        }

        sealed class LowercaseDummy
        {
            public int id { get; set; }
            public string value { get; set; }
        }

        sealed class NullableDummy
        {
            public int? ID { get; set; }
            public string Value { get; set; }
        }

        sealed class ConvertibleDummy
        {
            public decimal ID { get; set; }
            public string Value { get; set; }
        }

        sealed class NullableConvertibleDummy
        {
            public decimal? ID { get; set; }
            public string Value { get; set; }
        }

        sealed class Dummyz
        {
            public int IDz { get; set; }
            public string Valuez { get; set; }
        }

        sealed class EnumDummy
        {
            public int ID { get; set; }
            public ValueEnum Value { get; set; }
        }

        sealed class NullableEnumDummy
        {
            public int ID { get; set; }
            public ValueEnum? Value { get; set; }
        }

        sealed class IntValueDummy
        {
            public int ID { get; set; }
            public int Value { get; set; }
        }

        sealed class NullableIntValueDummy
        {
            public int ID { get; set; }
            public int? Value { get; set; }
        }

        enum ValueEnum
        {
            None,
            Test
        }
    }
}