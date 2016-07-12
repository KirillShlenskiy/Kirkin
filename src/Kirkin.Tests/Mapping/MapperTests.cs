﻿using System;

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
            MapperConfig<Dummy, LowercaseDummy> config1 = new MapperConfig<Dummy, LowercaseDummy> {
                MemberNameComparer = StringComparer.OrdinalIgnoreCase
            };

            MapperConfig<Dummy, LowercaseDummy> config2 = new MapperConfig<Dummy, LowercaseDummy> {
                MappingMode = MappingMode.Strict,
                MemberNameComparer = StringComparer.OrdinalIgnoreCase,
                NullableBehaviour = NullableBehaviour.DefaultMapsToNull
            };

            Assert.True(
                new MemberMappingCollection<Dummy, LowercaseDummy>(config1.ProduceValidMemberMappings()).Equals(
                    new MemberMappingCollection<Dummy, LowercaseDummy>(config2.ProduceValidMemberMappings())),
                "Maps are not equal."
            );

            config1.TargetMember(dst => dst.id).MapTo(src => src.ID + 1);

            Assert.False(
                new MemberMappingCollection<Dummy, LowercaseDummy>(config1.ProduceValidMemberMappings()).Equals(
                    new MemberMappingCollection<Dummy, LowercaseDummy>(config2.ProduceValidMemberMappings())),
                "Maps are equal."
            );
        }

        [Fact]
        public void CustomMemberMappingExpression()
        {
            var source = new Dummy { ID = 5, Value = "Test" };
            var target = new LowercaseDummy();

            var config = new MapperConfig<Dummy, LowercaseDummy> {
                MappingMode = MappingMode.AllTargetMembers,
                MemberNameComparer = StringComparer.OrdinalIgnoreCase
            };

            config.TargetMember(dst => dst.id).MapTo(src => src.ID + 1);
            config.TargetMember("value").MapTo(_ => "TEZD");

            Mapper.CreateMapper(config).Map(source, target);

            Assert.Equal(source.ID + 1, target.id);
            Assert.Equal("TEZD", target.value);
        }

        [Fact]
        public void CustomMemberMappingDelegate()
        {
            var source = new Dummy { ID = 5, Value = "Test" };
            var target = new LowercaseDummy();

            var config = new MapperConfig<Dummy, LowercaseDummy> {
                MappingMode = MappingMode.AllTargetMembers,
                MemberNameComparer = StringComparer.OrdinalIgnoreCase
            };

            config.TargetMember(dst => dst.id).MapWithDelegate(src => src.ID + 1);
            config.TargetMember(dst => dst.value).MapWithDelegate(_ => "TEZD");

            Mapper.CreateMapper(config).Map(source, target);

            Assert.Equal(source.ID + 1, target.id);
            Assert.Equal("TEZD", target.value);
        }

        [Fact]
        public void CustomUnmappedMemberMapping()
        {
            var config = new MapperConfig<Dummy, LowercaseDummy> { MappingMode = MappingMode.Relaxed };

            Assert.Equal(0, config.ProduceValidMemberMappings().Length);

            config.TargetMember(d => d.id).MapTo(d => d.ID);

            Assert.Equal(1, config.ProduceValidMemberMappings().Length);

            var target = Mapper.CreateMapper(config).Map(new Dummy { ID = 123 });

            Assert.Equal(123, target.id);
        }

        [Fact]
        public void ExecuteEmptyMapping()
        {
            var config = new MapperConfig<Dummy, LowercaseDummy> { MappingMode = MappingMode.Relaxed };

            Assert.Equal(0, config.ProduceValidMemberMappings().Length);
            Mapper.CreateMapper(config).Map(new Dummy());
        }

        [Fact]
        public void IgnoreMember()
        {
            var source = new Dummy { ID = 5, Value = "Test" };
            var config = new MapperConfig<Dummy, Dummy> { MappingMode = MappingMode.Relaxed };

            config.TargetMember(d => d.Value).Ignore();

            var target = Mapper.CreateMapper(config).Map(source);

            Assert.Equal(5, target.ID);
            Assert.Null(target.Value);
        }

        [Fact]
        public void IgnoreUnmappedMemberDoesNotThrow()
        {
            var config = new MapperConfig<Dummy, LowercaseDummy> { MappingMode = MappingMode.Relaxed };

            config.TargetMember(m => m.id).Ignore();
            config.Validate();

            config = new MapperConfig<Dummy, LowercaseDummy> { MappingMode = MappingMode.Relaxed };

            config.TargetMember(m => m.id).MapTo(config.SourceMember(m => m.ID).Member.Name);
            config.TargetMember(m => m.value).Ignore();
            config.Validate();
        }

        [Fact]
        public void ManualMapping()
        {
            var memberMappings = new MapperConfig<Dummy, LowercaseDummy> { MemberNameComparer = StringComparer.OrdinalIgnoreCase }
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
            var mappings = new MapperConfig<ExtendedDummy, Dummy> { MappingMode = MappingMode.AllTargetMembers }
                .ProduceValidMemberMappings();

            var compiler = new MappingCompiler<ExtendedDummy, Dummy>();

            for (int i = 0; i < 1000; i++) {
                compiler.CompileMapping(mappings);
            }
        }

        [Fact]
        public void CompileMapBenchmarkCached()
        {
            var mappings = new MapperConfig<ExtendedDummy, Dummy>() { MappingMode = MappingMode.AllTargetMembers }
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

            var dummy2 = Mapper
                .CreateMapper<Dummy, LowercaseDummy>(c => c.MemberNameComparer = StringComparer.OrdinalIgnoreCase)
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

            var config = new MapperConfig<Dummy, NullableDummy> {
                NullableBehaviour = NullableBehaviour.AssignDefaultAsIs,
                MappingMode = MappingMode.Relaxed
            };

            Mapper.CreateMapper(config).Map(dummy1, dummy2);
            Assert.Equal(0, dummy2.ID);
        }

        [Fact]
        public void NonNullableToNullableMappingError()
        {
            var dummy1 = new Dummy();
            var dummy2 = new NullableDummy { ID = 1 };
            var mapper = Mapper.CreateMapper<Dummy, NullableDummy>(c => c.NullableBehaviour = NullableBehaviour.Error);

            Assert.Throws<MappingException>(() => mapper.Map(dummy1, dummy2));
        }

        [Fact]
        public void MappingSucceeds()
        {
            var dummy = new Dummy { ID = 1, Value = "Test" };
            var extendedDummy = Mapper.CreateMapper<Dummy, ExtendedDummy>(c => c.MappingMode = MappingMode.Relaxed).Map(dummy);

            Assert.Equal(dummy.ID, extendedDummy.ID);
            Assert.Equal(dummy.Value, extendedDummy.Value);

            extendedDummy = Mapper.MapRelaxed<Dummy, ExtendedDummy>(dummy);

            Assert.Equal(dummy.ID, extendedDummy.ID);
            Assert.Equal(dummy.Value, extendedDummy.Value);

            Mapper.CreateMapper<Dummy, ExtendedDummy>(c => c.MappingMode = MappingMode.AllSourceMembers).Map(dummy, extendedDummy);
            Mapper.MapAllSourceMembers(dummy, extendedDummy);
            Mapper.CreateMapper<ExtendedDummy, Dummy>(c => c.MappingMode = MappingMode.AllTargetMembers).Map(extendedDummy, dummy);
            Mapper.MapAllTargetMembers(extendedDummy, dummy);
        }

        [Fact]
        public void MappingThrows()
        {
            var dummy = new Dummy();
            var extendedDummy = new ExtendedDummy();

            Assert.Throws<MappingException>(() =>
                new MapperConfig<Dummy, ExtendedDummy> { MappingMode = MappingMode.Strict }.Validate()
            );

            Assert.Throws<MappingException>(() => Mapper.MapStrict(new Dummy(), new ExtendedDummy()));

            Assert.Throws<MappingException>(() =>
                new MapperConfig<Dummy, ExtendedDummy> { MappingMode = MappingMode.AllTargetMembers }.Validate()
            );

            Assert.Throws<MappingException>(() => Mapper.MapAllTargetMembers(new Dummy(), new ExtendedDummy()));

            Assert.Throws<MappingException>(() =>
                new MapperConfig<ExtendedDummy, Dummy> { MappingMode = MappingMode.AllSourceMembers }.Validate()
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

            Mapper
                .CreateMapper<Dummy, NullableConvertibleDummy>(c => c.NullableBehaviour = NullableBehaviour.AssignDefaultAsIs)
                .Map(dummy, target);

            Assert.Equal(0, target.ID);
        }

        [Fact]
        public void StringToEnum()
        {
            var dummy = new Dummy { ID = 5, Value = "TEST" };
            var target = new EnumDummy();

            Mapper.MapStrict(dummy, target);

            Assert.Equal(ValueEnum.Test, target.Value);

            // Null string to Enum.
            dummy.Value = null;

            Assert.Throws<MappingException>(() => Mapper.MapStrict(dummy, target));
        }

        [Fact]
        public void StringToNullableEnum()
        {
            var dummy = new Dummy { ID = 5, Value = "TEST" };
            var target = new NullableEnumDummy();

            Mapper.MapStrict(dummy, target);

            Assert.Equal(ValueEnum.Test, target.Value);

            // Null string to Enum.
            dummy.Value = null;

            Mapper.MapStrict(dummy, target);

            Assert.False(target.Value.HasValue); // This is questionable behaviour.
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
        public void EnumToString()
        {
            var source = new EnumDummy { Value = ValueEnum.Test };
            var target = new Dummy();

            Mapper.MapStrict(source, target);

            Assert.Equal("Test", target.Value);
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
            var config = new MapperConfig<ExtendedDummy, Dummy>();
            var extendedDummy = new ExtendedDummy { ID = 123, Value = "Blah", Guid = Guid.NewGuid() };

            Assert.Throws<MappingException>(() => Mapper.CreateMapper(config).Map(extendedDummy));

            config.SourceMember(d => d.Guid).Ignore();

            var dummy = Mapper.CreateMapper(config).Map(extendedDummy);

            Assert.Equal(123, dummy.ID);
            Assert.Equal("Blah", dummy.Value);
        }

        [Fact]
        public void IgnorePreviouslyMappedMemberCausesValidationError()
        {
            var config = new MapperConfig<Dummy, Dummy>();

            config.TargetMember(d => d.ID).Ignore();

            Assert.Throws<MappingException>(() => config.Validate());

            config.TargetMember(d => d.ID).Reset();

            config.Validate();

            Assert.Equal(2, config.ProduceValidMemberMappings().Length);

            config.TargetMember(d => d.ID).Ignore(ignoreMatchingSource: true);

            Assert.Equal(1, config.ProduceValidMemberMappings().Length);

            config.Validate();
        }

        [Fact]
        public void TotalRemapWithStrictValidationByExpression()
        {
            var config = new MapperConfig<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => config.Validate());

            config.TargetMember(d => d.IDz).MapTo(d => d.ID);

            Assert.Throws<MappingException>(() => config.Validate());

            config.TargetMember(d => d.Valuez).MapTo(d => d.Value);

            config.Validate();
        }

        [Fact]
        public void TotalRemapWithStrictValidationByName()
        {
            var config = new MapperConfig<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => config.Validate());

            config.TargetMember("IDz").MapTo("ID");

            Assert.Throws<MappingException>(() => config.Validate());

            config.TargetMember("Valuez").MapTo("Value");

            config.Validate();
        }

        [Fact]
        public void MemberAccessHonoursConfigsNameComparer()
        {
            var config = new MapperConfig<Dummy, Dummyz>();

            Assert.Throws<MappingException>(() => config.SourceMember("id"));
            Assert.Throws<MappingException>(() => config.TargetMember("idz"));
            Assert.Throws<MappingException>(() => config.TargetMember("IDz").MapTo("id"));

            config.MemberNameComparer = StringComparer.OrdinalIgnoreCase;

            config.SourceMember("id");
            config.TargetMember("idz");
            config.TargetMember("IDz").MapTo("id");
        }

        [Fact]
        public void CustomMappingsHonourConfigsNullableBehaviour()
        {
            var config = new MapperConfig<Dummy, NullableDummy>();

            config.NullableBehaviour = NullableBehaviour.Error;

            Assert.Throws<MappingException>(() => Mapper.CreateMapper(config).Map(new Dummy())); // Value auto-mapped.

            config.TargetMember(d => d.ID).MapTo("ID");

            Assert.Throws<MappingException>(() => Mapper.CreateMapper(config).Map(new Dummy())); // Value manually mapped but nullable conversion fails.

            config.NullableBehaviour = NullableBehaviour.DefaultMapsToNull;

            var result = Mapper.CreateMapper(config).Map(new Dummy { ID = 0 }, new NullableDummy { ID = 123 });

            Assert.Null(result.ID);
        }

        [Fact]
        public void MapperFromTypeMapping()
        {
            Dummy d1 = new Dummy { ID = 123, Value = "Zzz" };

            Dummy d2 = Mapper
                .CreateMapper(PropertyList<Dummy>.Default.Without(d => d.Value))
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