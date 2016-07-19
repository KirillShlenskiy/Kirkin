using System;

using Kirkin.Mapping;
using Kirkin.Mapping.Engine.Compilers;
using Kirkin.Reflection;

using Xunit;

namespace Kirkin.Tests.Mapping
{
    public class MapperTests
    {
        [Fact]
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

        [Fact]
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

            Assert.Equal(source.ID + 1, target.id);
            Assert.Equal("TEZD", target.value);
        }

        [Fact]
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

            Assert.Equal(source.ID + 1, target.id);
            Assert.Equal("TEZD", target.value);
        }

        [Fact]
        public void CustomUnmappedMemberMapping()
        {
            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            Assert.Equal(0, builder.ProduceValidMemberMappings().Length);

            builder.TargetMember(d => d.id).MapTo(d => d.ID);

            Assert.Equal(1, builder.ProduceValidMemberMappings().Length);

            var target = builder.BuildMapper().Map(new Dummy { ID = 123 });

            Assert.Equal(123, target.id);
        }

        [Fact]
        public void ExecuteEmptyMapping()
        {
            var builder = new MapperBuilder<Dummy, LowercaseDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            Assert.Equal(0, builder.ProduceValidMemberMappings().Length);
            builder.BuildMapper().Map(new Dummy());
        }

        [Fact]
        public void IgnoreMember()
        {
            var source = new Dummy { ID = 5, Value = "Test" };

            var builder = new MapperBuilder<Dummy, Dummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            };

            builder.TargetMember(d => d.Value).Ignore();

            var target = builder.BuildMapper().Map(source);

            Assert.Equal(5, target.ID);
            Assert.Null(target.Value);
        }

        [Fact]
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

        [Fact]
        public void ManualMapping()
        {
            var memberMappings = new MapperBuilder<Dummy, LowercaseDummy> { MemberNameComparer = StringComparer.OrdinalIgnoreCase }
                .ProduceValidMemberMappings();

            var engine = new MappingCompiler<Dummy, LowercaseDummy>();
            var mappingDelegate = engine.CompileMapping(memberMappings);
            var source = new Dummy { ID = 5, Value = "Test" };
            var target = new LowercaseDummy();

            mappingDelegate(source, target);

            Assert.Equal(source.ID, target.id);
            Assert.Equal(source.Value, target.value);
        }

        [Fact]
        public void CompileMapBenchmark()
        {
            var mappings = new MapperBuilder<ExtendedDummy, Dummy> { AllowUnmappedSourceMembers = true }
                .ProduceValidMemberMappings();

            var compiler = new MappingCompiler<ExtendedDummy, Dummy>();

            for (int i = 0; i < 1000; i++) {
                compiler.CompileMapping(mappings);
            }
        }

        [Fact]
        public void CompileMapBenchmarkCached()
        {
            var mappings = new MapperBuilder<ExtendedDummy, Dummy>() { AllowUnmappedSourceMembers = true }
                .ProduceValidMemberMappings();

            var compiler = new CachedMappingCompiler<ExtendedDummy, Dummy>();

            for (int i = 0; i < 100000; i++) {
                compiler.CompileMapping(mappings);
            }
        }

        [Fact]
        public void MapSameType()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = Mapper.MapStrict<Dummy, Dummy>(dummy1);

            Assert.Equal(dummy1.ID, dummy2.ID);
            Assert.Equal(dummy1.Value, dummy2.Value);
        }

        [Fact]
        public void MapDerivedType()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new ExtendedDummy();

            Mapper.Map(dummy1, dummy2);

            Assert.Equal(dummy1.ID, dummy2.ID);
            Assert.Equal(dummy1.Value, dummy2.Value);
        }

        [Fact]
        public void MapDerivedTypeDynamic()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new ExtendedDummy();

            Mapper.MapStrict<Dummy, Dummy>(dummy1, dummy2);

            Assert.Equal(dummy1.ID, dummy2.ID);
            Assert.Equal(dummy1.Value, dummy2.Value);
        }

        [Fact]
        public void MapperStrictByDefault()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new ExtendedDummy();

            Assert.Throws<MappingException>(() => Mapper.MapStrict(dummy1, dummy2));
        }

        [Fact]
        public void MapperCaseSensitiveByDefault()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };
            var dummy2 = new LowercaseDummy();

            Assert.Throws<MappingException>(() => Mapper.MapStrict(dummy1, dummy2));
        }

        [Fact]
        public void MapDifferentTypes()
        {
            var dummy1 = new Dummy { ID = 1, Value = "Test" };

            var dummy2 = new MapperBuilder<Dummy, LowercaseDummy> { MemberNameComparer = StringComparer.OrdinalIgnoreCase }
                .BuildMapper()
                .Map(dummy1);

            Assert.Equal(dummy1.ID, dummy2.id);
            Assert.Equal(dummy1.Value, dummy2.value);
        }

        [Fact]
        public void ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => Mapper.MapStrict<object, object>(null, null));
        }

        [Fact]
        public void NullableToNonNullableMapping()
        {
            var dummy1 = new NullableDummy();
            var dummy2 = new Dummy { ID = 1 };

            Mapper.MapStrict(dummy1, dummy2);
            Assert.Equal(0, dummy2.ID);
        }

        [Fact]
        public void NonNullableToNullableMapping()
        {
            var dummy1 = new Dummy();
            var dummy2 = new NullableDummy { ID = 1 };

            Mapper.MapStrict(dummy1, dummy2);
            Assert.Null(dummy2.ID);
        }

        [Fact]
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
            Assert.Equal(0, dummy2.ID);
        }

        [Fact]
        public void NonNullableToNullableMappingError()
        {
            var dummy1 = new Dummy();
            var dummy2 = new NullableDummy { ID = 1 };
            var mapper = new MapperBuilder<Dummy, NullableDummy> { NullableBehaviour = NullableBehaviour.Error }.BuildMapper();

            Assert.Throws<MappingException>(() => mapper.Map(dummy1, dummy2));
        }

        [Fact]
        public void MappingSucceeds()
        {
            var dummy = new Dummy { ID = 1, Value = "Test" };

            var extendedDummy = new MapperBuilder<Dummy, ExtendedDummy> {
                AllowUnmappedSourceMembers = true,
                AllowUnmappedTargetMembers = true
            }
            .BuildMapper()
            .Map(dummy);

            Assert.Equal(dummy.ID, extendedDummy.ID);
            Assert.Equal(dummy.Value, extendedDummy.Value);

            extendedDummy = Mapper.MapRelaxed<Dummy, ExtendedDummy>(dummy);

            Assert.Equal(dummy.ID, extendedDummy.ID);
            Assert.Equal(dummy.Value, extendedDummy.Value);

            new MapperBuilder<Dummy, ExtendedDummy> { AllowUnmappedTargetMembers = true }.BuildMapper().Map(dummy, extendedDummy);
            Mapper.MapAllSourceMembers(dummy, extendedDummy);
            new MapperBuilder<ExtendedDummy, Dummy> { AllowUnmappedSourceMembers = true }.BuildMapper().Map(extendedDummy, dummy);
            Mapper.MapAllTargetMembers(extendedDummy, dummy);
        }

        [Fact]
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

        [Fact]
        public void ConvertibleSupport()
        {
            var dummy = new Dummy { ID = 5 };
            var target = Mapper.MapStrict(dummy, new ConvertibleDummy());

            Assert.Equal(5m, target.ID);
        }

        [Fact]
        public void NullableConvertibleSupport()
        {
            var dummy = new Dummy { ID = 5 };
            var target = Mapper.MapStrict(dummy, new NullableConvertibleDummy());

            Assert.Equal(5m, target.ID);

            dummy.ID = 0;

            Mapper.MapStrict(dummy, target);

            Assert.False(target.ID.HasValue);

            new MapperBuilder<Dummy, NullableConvertibleDummy> { NullableBehaviour = NullableBehaviour.AssignDefaultAsIs }
                .BuildMapper()
                .Map(dummy, target);

            Assert.Equal(0, target.ID);
        }

        [Fact]
        public void StringToEnum()
        {
            Assert.Equal(ValueEnum.Test, Mapper.MapStrict(new Dummy { Value = "TEST" }, new EnumDummy()).Value);
            Assert.Equal(ValueEnum.None, Mapper.MapStrict(new Dummy { Value = null }, new EnumDummy()).Value);
            Assert.Throws<ArgumentException>(() => Mapper.MapStrict(new Dummy { Value = "" }, new EnumDummy()));
        }
            
        [Fact]
        public void StringToNullableEnum()
        {
            Assert.Equal(ValueEnum.Test, Mapper.MapStrict(new Dummy { Value = "TEST" }, new NullableEnumDummy()).Value);
            Assert.Null(Mapper.MapStrict(new Dummy { Value = null }, new NullableEnumDummy()).Value);
            Assert.Throws<ArgumentException>(() => Mapper.MapStrict(new Dummy { Value = "" }, new NullableEnumDummy()));
        }

        [Fact]
        public void EnumToString()
        {
            Assert.Equal("Test", Mapper.MapStrict(new EnumDummy { Value = ValueEnum.Test }, new Dummy()).Value);
            Assert.Equal("None", Mapper.MapStrict(new EnumDummy(), new Dummy()).Value);
        }

        [Fact]
        public void NullableEnumToString()
        {
            Assert.Equal("Test", Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.Test }, new Dummy()).Value);
            Assert.Equal("None", Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.None }, new Dummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableEnumDummy { Value = null }, new Dummy()).Value);
        }

        [Fact]
        public void IntToEnum()
        {
            Assert.Equal(ValueEnum.Test, Mapper.MapStrict(new IntValueDummy { Value = 1 }, new EnumDummy()).Value);
            Assert.Equal(ValueEnum.None, Mapper.MapStrict(new IntValueDummy { Value = 0 }, new EnumDummy()).Value);
        }

        [Fact]
        public void IntToNullableEnum()
        {
            Assert.Equal(ValueEnum.Test, Mapper.MapStrict(new IntValueDummy { Value = 1 }, new NullableEnumDummy()).Value);
            Assert.Null(Mapper.MapStrict(new IntValueDummy { Value = 0 }, new NullableEnumDummy()).Value);
        }

        [Fact]
        public void NullableIntToEnum()
        {
            Assert.Equal(ValueEnum.Test, Mapper.MapStrict(new NullableIntValueDummy { Value = 1 }, new EnumDummy()).Value);
            Assert.Equal(ValueEnum.None, Mapper.MapStrict(new NullableIntValueDummy { Value = 0 }, new EnumDummy()).Value);
            Assert.Equal(ValueEnum.None, Mapper.MapStrict(new NullableIntValueDummy { Value = null }, new EnumDummy()).Value);
        }

        [Fact]
        public void NullableIntToNullableEnum()
        {
            Assert.Equal(ValueEnum.Test, Mapper.MapStrict(new NullableIntValueDummy { Value = 1 }, new NullableEnumDummy()).Value);
            Assert.Equal(ValueEnum.None, Mapper.MapStrict(new NullableIntValueDummy { Value = 0 }, new NullableEnumDummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableIntValueDummy { Value = null }, new NullableEnumDummy()).Value);
        }

        [Fact]
        public void EnumToInt()
        {
            Assert.Equal(1, Mapper.MapStrict(new EnumDummy { Value = ValueEnum.Test }, new IntValueDummy()).Value);
            Assert.Equal(0, Mapper.MapStrict(new EnumDummy { Value = ValueEnum.None }, new IntValueDummy()).Value);
        }

        [Fact]
        public void EnumToNullableInt()
        {
            Assert.Equal(1, Mapper.MapStrict(new EnumDummy { Value = ValueEnum.Test }, new NullableIntValueDummy()).Value);
            Assert.Null(Mapper.MapStrict(new EnumDummy { Value = ValueEnum.None }, new NullableIntValueDummy()).Value);
        }

        [Fact]
        public void NullableEnumToInt()
        {
            Assert.Equal(1, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.Test }, new IntValueDummy()).Value);
            Assert.Equal(0, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.None }, new IntValueDummy()).Value);
            Assert.Equal(0, Mapper.MapStrict(new NullableEnumDummy { Value = null }, new IntValueDummy()).Value);
        }

        [Fact]
        public void NullableEnumToNullableInt()
        {
            Assert.Equal(1, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.Test }, new NullableIntValueDummy()).Value);
            Assert.Equal(0, Mapper.MapStrict(new NullableEnumDummy { Value = ValueEnum.None }, new NullableIntValueDummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableEnumDummy { Value = null }, new NullableIntValueDummy()).Value);
        }

        [Fact]
        public void IntToString()
        {
            Assert.Equal("0", Mapper.MapStrict(new IntValueDummy { Value = 0 }, new Dummy()).Value);
            Assert.Equal("0", Mapper.MapStrict(new NullableIntValueDummy { Value = 0 }, new Dummy()).Value);
            Assert.Null(Mapper.MapStrict(new NullableIntValueDummy { Value = null }, new Dummy()).Value);
        }

        [Fact]
        public void NullStringToNullableInt()
        {
            // Strict mapping: even though we could technically execute it
            // for the case where Value = null, all other cases would fail,
            // so going from string -> Nullable<T> is prohibited.
            Assert.Throws<InvalidOperationException>(() => Mapper.MapStrict(new Dummy { Value = null }, new NullableIntValueDummy()));
        }

        [Fact]
        public void StructMapping()
        {
            var source = new Size { Width = 2, Height = 4 };
            var target = Mapper.MapStrict(source, new Size());

            Assert.Equal(2, target.Width);
            Assert.Equal(4, target.Height);
        }

        [Fact]
        public void SourceMemberIgnore()
        {
            var builder = new MapperBuilder<ExtendedDummy, Dummy>();
            var extendedDummy = new ExtendedDummy { ID = 123, Value = "Blah", Guid = Guid.NewGuid() };

            Assert.Throws<MappingException>(() => builder.BuildMapper().Map(extendedDummy));

            builder.SourceMember(d => d.Guid).Ignore();

            var dummy = builder.BuildMapper().Map(extendedDummy);

            Assert.Equal(123, dummy.ID);
            Assert.Equal("Blah", dummy.Value);
        }

        [Fact]
        public void IgnorePreviouslyMappedMemberCausesValidationError()
        {
            var builder = new MapperBuilder<Dummy, Dummy>();

            builder.TargetMember(d => d.ID).Ignore();

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember(d => d.ID).Reset();

            builder.ValidateMapping();

            Assert.Equal(2, builder.ProduceValidMemberMappings().Length);

            builder.TargetMember(d => d.ID).Ignore(ignoreMatchingSource: true);

            Assert.Equal(1, builder.ProduceValidMemberMappings().Length);

            builder.ValidateMapping();
        }

        [Fact]
        public void TotalRemapWithStrictValidationByExpression()
        {
            var builder = new MapperBuilder<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember(d => d.IDz).MapTo(d => d.ID);

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember(d => d.Valuez).MapTo(d => d.Value);

            builder.ValidateMapping();
        }

        [Fact]
        public void TotalRemapWithStrictValidationByName()
        {
            var builder = new MapperBuilder<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember("IDz").MapTo("ID");

            Assert.Throws<MappingException>(() => builder.ValidateMapping());

            builder.TargetMember("Valuez").MapTo("Value");

            builder.ValidateMapping();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void MapperFromTypeMapping()
        {
            Dummy d1 = new Dummy { ID = 123, Value = "Zzz" };

            Dummy d2 = Mapper.Builder
                .From(PropertyMember.PropertyListMembers(PropertyList<Dummy>.Default.Without(d => d.Value)))
                .To(PropertyMember.PropertyListMembers(PropertyList<Dummy>.Default.Without(d => d.Value)))
                .BuildMapper()
                .Map(d1);

            Assert.Equal(123, d2.ID);
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