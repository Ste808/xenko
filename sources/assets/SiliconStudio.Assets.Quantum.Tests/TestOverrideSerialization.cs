﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public static class IdentifierGenerator
    {
        public static ItemId Get(int index)
        {
            var bytes = ToBytes(index);
            return new ItemId(bytes);
        }

        public static bool Match(ItemId guid, int index)
        {
            var bytes = ToBytes(index);
            var id = new ItemId(bytes);
            return guid == id;
        }

        private static byte[] ToBytes(int index)
        {
            var bytes = new byte[16];
            for (int i = 0; i < 4; ++i)
            {
                bytes[4 * i] = (byte)(index);
                bytes[4 * i + 1] = (byte)(index >> 8);
                bytes[4 * i + 2] = (byte)(index >> 16);
                bytes[4 * i + 3] = (byte)(index >> 24);
            }
            return bytes;
        }
    }

    [TestFixture]
    public class TestOverrideSerialization
    {
        /* test TODO:
         * Non-abstract class (test result recursively) : simple prop + in collection
         * Abstract (interface) override with different type
         * class prop set to null
         */

        private static void SerializeAndCompare(AssetItem assetItem, AssetPropertyGraph graph, string expectedYaml)
        {
            assetItem.Asset.Id = Guid.Empty;
            if (assetItem.Asset.Base != null)
            {
                assetItem.Asset.Base.Asset.Id = Guid.Empty;
            }
            graph.UpdateOverridesForSerialization();
            var stream = new MemoryStream();
            AssetSerializer.Save(stream, assetItem.Asset, null, (Dictionary<ObjectPath, OverrideType>)assetItem.Overrides);
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            var yaml = streamReader.ReadToEnd();
            Assert.AreEqual(expectedYaml, yaml);
        }

        private const string SimplePropertyUpdateBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyString: MyBaseString
";
        private const string SimplePropertyUpdateDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyString*: MyDerivedString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        MyString: String
";
        private const string SimpleCollectionUpdateBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    14000000140000001400000014000000: MyBaseString
";
        private const string SimpleCollectionUpdateDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000*: MyDerivedString
    14000000140000001400000014000000: MyBaseString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings: {}
        MyStrings:
            0a0000000a0000000a0000000a000000: String1
            14000000140000001400000014000000: String2
";
        private const string SimpleDictionaryUpdateBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    14000000140000001400000014000000~Key2: MyBaseString
";
        private const string SimpleDictionaryUpdateDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1*: MyDerivedString
    14000000140000001400000014000000~Key2: MyBaseString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
        private const string CollectionInStructBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings:
        0a0000000a0000000a0000000a000000: String1
        14000000140000001400000014000000: MyBaseString
MyStrings: {}
";
        private const string CollectionInStructDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings:
        0a0000000a0000000a0000000a000000*: MyDerivedString
        14000000140000001400000014000000: MyBaseString
MyStrings: {}
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings:
                0a0000000a0000000a0000000a000000: String1
                14000000140000001400000014000000: String2
        MyStrings: {}
";
        private const string SimpleCollectionAddBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    14000000140000001400000014000000: String2
    {0}: String4
";
        private const string SimpleCollectionAddDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    14000000140000001400000014000000: String2
    {0}: String4
    {1}*: String3
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings: {}
        MyStrings:
            0a0000000a0000000a0000000a000000: String1
            14000000140000001400000014000000: String2
";
        private const string SimpleDictionaryAddBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    14000000140000001400000014000000~Key2: String2
    {0}~Key4: String4
";
        private const string SimpleDictionaryAddDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    14000000140000001400000014000000~Key2: String2
    {1}~Key3*: String3
    {0}~Key4: String4
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
        private const string ObjectCollectionUpdateBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset4,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyObjects:
    0a0000000a0000000a0000000a000000:
        Value: String1
    14000000140000001400000014000000:
        Value: MyBaseString
";
        private const string ObjectCollectionUpdateDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset4,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyObjects:
    0a0000000a0000000a0000000a000000*:
        Value: MyDerivedString
    14000000140000001400000014000000:
        Value: MyBaseString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset4,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        MyObjects:
            0a0000000a0000000a0000000a000000:
                Value: String1
            14000000140000001400000014000000:
                Value: String2
";
        private const string ObjectCollectionAddBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset4,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyObjects:
    0a0000000a0000000a0000000a000000:
        Value: String1
    14000000140000001400000014000000:
        Value: String2
    {0}:
        Value: String4
";
        private const string ObjectCollectionAddDerivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset4,SiliconStudio.Assets.Quantum.Tests
Id: 00000000-0000-0000-0000-000000000000
Tags: []
MyObjects:
    0a0000000a0000000a0000000a000000:
        Value: String1
    14000000140000001400000014000000:
        Value: String2
    {0}:
        Value: String4
    {1}*:
        Value: String3
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset4,SiliconStudio.Assets.Quantum.Tests
        Id: 00000000-0000-0000-0000-000000000000
        Tags: []
        MyObjects:
            0a0000000a0000000a0000000a000000:
                Value: String1
            14000000140000001400000014000000:
                Value: String2
";

        [Test]
        public void TestSimplePropertySerialization()
        {
            var asset = new Types.MyAsset1 { MyString = "String" };
            var context = DeriveAssetTest<Types.MyAsset1>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));

            basePropertyNode.Content.Update("MyBaseString");
            derivedPropertyNode.Content.Update("MyDerivedString");
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, SimplePropertyUpdateBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, SimplePropertyUpdateDerivedYaml);

            context = DeriveAssetTest<Types.MyAsset1>.LoadFromYaml(SimplePropertyUpdateBaseYaml, SimplePropertyUpdateDerivedYaml);
            basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));
            derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));

            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(Index.Empty));
        }

        [Test]
        public void TestSimplePropertyDeserialization()
        {
            var context = DeriveAssetTest<Types.MyAsset1>.LoadFromYaml(SimplePropertyUpdateBaseYaml, SimplePropertyUpdateDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));

            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(Index.Empty));
        }

        [Test]
        public void TestSimpleCollectionUpdateSerialization()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            var ids = CollectionItemIdHelper.GetCollectionItemIds(asset.MyStrings);
            ids.Add(0, IdentifierGenerator.Get(10));
            ids.Add(1, IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));

            basePropertyNode.Content.Update("MyBaseString", new Index(1));
            derivedPropertyNode.Content.Update("MyDerivedString", new Index(0));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, SimpleCollectionUpdateBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, SimpleCollectionUpdateDerivedYaml);
        }

        [Test]
        public void TestSimpleCollectionUpdateDeserialization()
        {
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(SimpleCollectionUpdateBaseYaml, SimpleCollectionUpdateDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);

            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestSimpleDictionaryUpdateSerialization()
        {
            var asset = new Types.MyAsset3 { MyDictionary = { { "Key1", "String1" }, { "Key2", "String2" } } };
            var ids = CollectionItemIdHelper.GetCollectionItemIds(asset.MyDictionary);
            ids.Add("Key1", IdentifierGenerator.Get(10));
            ids.Add("Key2", IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset3>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));

            basePropertyNode.Content.Update("MyBaseString", new Index("Key2"));
            derivedPropertyNode.Content.Update("MyDerivedString", new Index("Key1"));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, SimpleDictionaryUpdateBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, SimpleDictionaryUpdateDerivedYaml);

            context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(SimpleDictionaryUpdateBaseYaml, SimpleDictionaryUpdateDerivedYaml);
            basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);

            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
        }

        [Test]
        public void TestSimpleDictionaryDeserialization()
        {
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(SimpleDictionaryUpdateBaseYaml, SimpleDictionaryUpdateDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);

            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
        }

        [Test]
        public void TestCollectionInStructUpdateSerialization()
        {
            var asset = new Types.MyAsset2();
            asset.Struct.MyStrings.Add("String1");
            asset.Struct.MyStrings.Add("String2");
            var ids = CollectionItemIdHelper.GetCollectionItemIds(asset.Struct.MyStrings);
            ids.Add(0, IdentifierGenerator.Get(10));
            ids.Add(1, IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.Struct)).GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.Struct)).GetChild(nameof(Types.MyAsset2.MyStrings));

            basePropertyNode.Content.Update("MyBaseString", new Index(1));
            derivedPropertyNode.Content.Update("MyDerivedString", new Index(0));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, CollectionInStructBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, CollectionInStructDerivedYaml);        }

        [Test]
        public void TestCollectionInStructUpdateDeserialization()
        {
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(CollectionInStructBaseYaml, CollectionInStructDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.Struct)).GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.Struct)).GetChild(nameof(Types.MyAsset2.MyStrings));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.Struct.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.Struct.MyStrings);

            Assert.AreEqual(2, context.BaseAsset.Struct.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.Struct.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestSimpleCollectionAddSerialization()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyStrings);
            baseIds.Add(0, IdentifierGenerator.Get(10));
            baseIds.Add(1, IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));

            derivedPropertyNode.Content.Add("String3");
            basePropertyNode.Content.Add("String4");
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var expectedBaseYaml = string.Format(SimpleCollectionAddBaseYaml.Replace("{}", "{{}}"), baseIds.GetId(2));
            var expectedDerivedYaml = string.Format(SimpleCollectionAddDerivedYaml.Replace("{}", "{{}}"), baseIds.GetId(2), derivedIds.GetId(3));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, expectedBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, expectedDerivedYaml);
        }

        [Test]
        public void TestSimpleCollectionAddDeserialization()
        {
            var expectedBaseYaml = string.Format(SimpleCollectionAddBaseYaml.Replace("{}", "{{}}"), IdentifierGenerator.Get(30));
            var expectedDerivedYaml = string.Format(SimpleCollectionAddDerivedYaml.Replace("{}", "{{}}"), IdentifierGenerator.Get(30), IdentifierGenerator.Get(40));
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(expectedBaseYaml, expectedDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);

            Assert.AreEqual(3, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(2)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(3)));
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[2]);
        }

        [Test]
        public void TestSimpleDictionaryAddSerialization()
        {
            var asset = new Types.MyAsset3 { MyDictionary = { { "Key1", "String1" }, { "Key2", "String2" } } };
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyDictionary);
            baseIds.Add("Key1", IdentifierGenerator.Get(10));
            baseIds.Add("Key2", IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset3>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));

            // Update derived and check
            derivedPropertyNode.Content.Add("String3", new Index("Key3"));
            basePropertyNode.Content.Add("String4", new Index("Key4"));

            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var expectedBaseYaml = string.Format(SimpleDictionaryAddBaseYaml.Replace("{}", "{{}}"), baseIds.GetId("Key4"));
            var expectedDerivedYaml = string.Format(SimpleDictionaryAddDerivedYaml.Replace("{}", "{{}}"), baseIds.GetId("Key4"), derivedIds.GetId("Key3"));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, expectedBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, expectedDerivedYaml);
        }

        [Test]
        public void TestSimpleDictionaryAddDeserialization()
        {
            var expectedBaseYaml = string.Format(SimpleDictionaryAddBaseYaml.Replace("{}", "{{}}"), IdentifierGenerator.Get(30));
            var expectedDerivedYaml = string.Format(SimpleDictionaryAddDerivedYaml.Replace("{}", "{{}}"), IdentifierGenerator.Get(30), IdentifierGenerator.Get(40));
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(expectedBaseYaml, expectedDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);

            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index("Key4")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index("Key4")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key4")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key4")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
            Assert.AreEqual(baseIds["Key4"], derivedIds["Key4"]);
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyDictionary.Count);
        }

        [Test]
        public void TestObjectCollectionUpdateSerialization()
        {
            var asset = new Types.MyAsset4 { MyObjects = { new Types.SomeObject { Value = "String1" }, new Types.SomeObject { Value = "String2" } } };
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyObjects);
            baseIds.Add(0, IdentifierGenerator.Get(10));
            baseIds.Add(1, IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset4>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));

            basePropertyNode.Content.Update(new Types.SomeObject { Value = "MyBaseString" }, new Index(1));
            derivedPropertyNode.Content.Update(new Types.SomeObject { Value = "MyDerivedString" }, new Index(0));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, ObjectCollectionUpdateBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, ObjectCollectionUpdateDerivedYaml);
        }

        [Test]
        public void TestObjectCollectionUpdateDeserialization()
        {
            var context = DeriveAssetTest<Types.MyAsset4>.LoadFromYaml(ObjectCollectionUpdateBaseYaml, ObjectCollectionUpdateDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyObjects);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyObjects);

            Assert.AreEqual(2, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("MyDerivedString", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestObjectCollectionAddSerialization()
        {
            var asset = new Types.MyAsset4 { MyObjects = { new Types.SomeObject { Value = "String1" }, new Types.SomeObject { Value = "String2" } } };
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyObjects);
            baseIds.Add(0, IdentifierGenerator.Get(10));
            baseIds.Add(1, IdentifierGenerator.Get(20));
            var context = DeriveAssetTest<Types.MyAsset4>.DeriveAsset(asset);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyObjects);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));

            derivedPropertyNode.Content.Add(new Types.SomeObject { Value = "String3" });
            basePropertyNode.Content.Add(new Types.SomeObject { Value = "String4" });
            var expectedBaseYaml = string.Format(ObjectCollectionAddBaseYaml.Replace("{}", "{{}}"), baseIds.GetId(2));
            var expectedDerivedYaml = string.Format(ObjectCollectionAddDerivedYaml.Replace("{}", "{{}}"), baseIds.GetId(2), derivedIds.GetId(3));
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, expectedBaseYaml);
            SerializeAndCompare(context.DerivedAssetItem, context.DerivedGraph, expectedDerivedYaml);
        }

        [Test]
        public void TestObjectCollectionAddDeserialization()
        {
            var expectedBaseYaml = string.Format(ObjectCollectionAddBaseYaml.Replace("{}", "{{}}"), IdentifierGenerator.Get(30));
            var expectedDerivedYaml = string.Format(ObjectCollectionAddDerivedYaml.Replace("{}", "{{}}"), IdentifierGenerator.Get(30), IdentifierGenerator.Get(40));
            var context = DeriveAssetTest<Types.MyAsset4>.LoadFromYaml(expectedBaseYaml, expectedDerivedYaml);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset4.MyObjects));
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyObjects);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyObjects);

            Assert.AreEqual(3, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String4", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String4", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual("String3", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(3))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(2)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(3)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(3)].TargetNode.GetChild(nameof(Types.SomeObject.Value))).GetOverride(Index.Empty));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[2]);
        }
    }
}
