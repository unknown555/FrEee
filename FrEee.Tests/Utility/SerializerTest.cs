﻿using FrEee.Objects.Abilities;
using FrEee.Objects.Civilization;
using FrEee.Objects.Technology;
using FrEee.Objects.Vehicles;
using FrEee.Modding;
using FrEee.Utility;
using FrEee.Serialization;
using FrEee.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FrEee.Objects.Civilization.Orders;
using FrEee.Data.Models;
using System;

namespace FrEee.Tests.Utility;

public class SerializerTest
{
	[OneTimeSetUp]
	public static void ClassInit()
	{
		// silly unit tests can't find their own assembly
		SafeType.ForceLoadType(typeof(SerializerTest));
		SafeType.ForceLoadType(typeof(Car));
		SafeType.ForceLoadType(typeof(Company));
	}

	[Test]
	public void CircularReferences()
	{
		var george = new Person("George");
		var brad = new Person("Brad");
		george.Partner = brad;
		brad.Partner = george;
		var s = Serializer.SerializeToString(george);
		var george2 = Serializer.DeserializeFromString<Person>(s);
		Assert.AreEqual(george.ToString(), george2.ToString());
	}

	[Test]
	[Ignore("Point is no longer a magic type")]
	public void MagicTypes()
	{
		var p = new System.Drawing.Point(6, 9);
		var s = Serializer.SerializeToString(p);
		var p2 = Serializer.DeserializeFromString(s);
		Assert.AreEqual(p, p2);
	}

	[Test]
	public void Roundtrip()
	{
		var chevy = new Company("Chevrolet");
		var car = new Car(chevy, "HHR", 2007);
		var s = Serializer.SerializeToString(car);
		var car2 = Serializer.DeserializeFromString<Car>(s);
		Assert.AreEqual(car.ToString(), car2.ToString());
	}

	[Test]
	public void Scalar()
	{
		var answer = 42;
		var s = Serializer.SerializeToString(answer);
		var answer2 = Serializer.DeserializeFromString(s);
		Assert.AreEqual(answer, answer2);
	}

	[Test]
	public void NestedDictionary()
	{
		var dict = new SafeDictionary<string, SafeDictionary<string, string>>(true);
		dict["test"]["fred"] = "flintstone";
		var s = Serializer.SerializeToString(dict);
		var dict2 = Serializer.DeserializeFromString<SafeDictionary<string, SafeDictionary<string, string>>>(s);
		Assert.AreEqual(dict["test"]["fred"], dict2["test"]["fred"]);
	}

	[Test]
	public void NestedCrossAssembly()
	{
		var list = new List<Formula<string>>();
		list.Add(new LiteralFormula<string>("hello"));
		var s = Serializer.SerializeToString(list);
		var list2 = Serializer.DeserializeFromString<List<Formula<string>>>(s);
		Assert.AreEqual(list.First(), list2.First());
	}

	[Test]
	public void NestedCrossAssemblySafeTypeName()
	{
		var s = SafeType.GetShortTypeName(typeof(List<Formula<string>>));
		Assert.AreEqual("System.Collections.Generic.List`1[[FrEee.Modding.Formula`1[[System.String, System.Private.CoreLib]], FrEee.Core]], System.Private.CoreLib", s);
	}

	[Test]
	public void NestedDictionaries()
	{
		var s = SafeType.GetShortTypeName(typeof(IDictionary<AbilityRule, IDictionary<int, Formula<int>>>));
		Assert.AreEqual("System.Collections.Generic.IDictionary`2[[FrEee.Objects.Abilities.AbilityRule, FrEee.Core],[System.Collections.Generic.IDictionary`2[[System.Int32, System.Private.CoreLib],[FrEee.Modding.Formula`1[[System.Int32, System.Private.CoreLib]], FrEee.Core]], System.Private.CoreLib]], System.Private.CoreLib", s);
	}

	[Test]
	public void LookUpComplexType()
	{
		{
			var tname = "System.Collections.Generic.List`1[[FrEee.Modding.Formula`1[[System.String, System.Private.CoreLib]], FrEee.Core]], System.Private.CoreLib";
			var st = new SafeType(tname);
			Assert.AreEqual(typeof(List<Formula<string>>), st.Type);
		}
		{
			var tname = "FrEee.Objects.Orders.ConstructionOrder`2[[FrEee.Objects.Vehicles.Ship, FrEee.Core],[FrEee.Objects.Vehicles.Design`1[[FrEee.Objects.Vehicles.Ship, FrEee.Core]], FrEee.Core";
			var st = new SafeType(tname);
			Assert.AreEqual(typeof(ConstructionOrder<Ship, Design<Ship>>), st.Type);
		}
		{
			var tname = "System.Collections.Generic.Dictionary`2[[FrEee.Objects.Abilities.AbilityRule, FrEee.Core],[FrEee.Modding.Formula`1[[System.Int32, System.Private.CoreLib]], System.Private.CoreLib";
			var st = new SafeType(tname);
			Assert.AreEqual(typeof(Dictionary<AbilityRule, Formula<int>>), st.Type);
		}
		{
			var tname = "System.Collections.Generic.IDictionary`2[[FrEee.Objects.Abilities.AbilityRule, FrEee.Core, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null],[System.Collections.Generic.IDictionary`2[[System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[FrEee.Modding.Formula`1[[System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], FrEee.Core, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";
			var st = new SafeType(tname);
			Assert.AreEqual(typeof(IDictionary<AbilityRule, IDictionary<int, Formula<int>>>), st.Type);
		}
	}

	[Test]
	public void LegacyLookUpComplexType()
	{
		{
			var tname = "System.Collections.Generic.List`1[[FrEee.Objects.Abilities.AbilityRule, FrEee.Core, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			var st = new SafeType(tname);
			Assert.AreEqual(typeof(List<AbilityRule>), st.Type);
		}
		{
			var tname = "System.Collections.Generic.List`1[[FrEee.Modding.Formula`1[[System.String, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], FrEee.Core, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			var st = new SafeType(tname);
			Assert.AreEqual(typeof(List<Formula<string>>), st.Type);
		}
	}

	[Test]
	[Ignore("Script type no longer in existence")]
	public void LegacyLookUpArrays()
	{
		var tname = "FrEee.Modding.Script[], FrEee.Core, Version=0.0.9.0, Culture=neutral, PublicKeyToken=null";
		var st = new SafeType(tname);
		Assert.AreEqual(typeof(PythonScript[]), st.Type);
	}

	[Test]
	public void CorrectAbilities()
	{
		Mod.Load(null);
		var ft1 = new FacilityTemplate();
		ft1.Name = "Mineral Miner Test";
		ft1.Abilities.Add(new Ability(ft1, Mod.Current.AbilityRules.FindByName("Resource Generation - Minerals"), null, 800));
		var ft2 = new FacilityTemplate();
		ft2.Name = "Organics Farm Test";
		ft2.Abilities.Add(new Ability(ft1, Mod.Current.AbilityRules.FindByName("Resource Generation - Organics"), null, 800));
		var fts = new List<FacilityTemplate> { ft1, ft2 };
		var serdata = Serializer.SerializeToString(fts);
		var deser = Serializer.Deserialize<List<FacilityTemplate>>(serdata);
		Assert.AreEqual(800, deser.First().GetAbilityValue("Resource Generation - Minerals").ToInt());
		Assert.AreEqual(0, deser.First().GetAbilityValue("Resource Generation - Organics").ToInt());
		Assert.AreEqual(0, deser.Last().GetAbilityValue("Resource Generation - Minerals").ToInt());
		Assert.AreEqual(800, deser.Last().GetAbilityValue("Resource Generation - Organics").ToInt());
	}


	[Test]
	public void CorrectAbilities2()
	{
		Mod.Load(null);
		var serdata = Serializer.SerializeToString(Mod.Current);
		Mod.Current = Serializer.DeserializeFromString<Mod>(serdata);
		Assert.AreEqual(800, Mod.Current.FacilityTemplates.Single(x => x.Name == "Mineral Miner Facility I").GetAbilityValue("Resource Generation - Minerals").ToInt());
	}

	[Test]
	public void EmpireStoredResources()
	{
		var emp = new Empire();
		emp.StoredResources = 50000 * Resource.Minerals + 50000 * Resource.Organics + 50000 * Resource.Radioactives;
		var serdata = Serializer.SerializeToString(emp);
		var emp2 = Serializer.DeserializeFromString<Empire>(serdata);
		Assert.AreEqual(emp.StoredResources, emp2.StoredResources);
	}

	[Test]
	public void QuotedStrings()
	{
		var str = "\"Hello world!\"";
		var serdata = Serializer.SerializeToString(str);
		var str2 = Serializer.DeserializeFromString<string>(serdata);
		Assert.AreEqual(str, str2);
	}

	[Test]
	public void PrivateProperties()
	{
		var spy = new Spy();
		spy.SetSecretCode(42000);
		var serdata = Serializer.SerializeToString(spy);
		var spy2 = Serializer.DeserializeFromString<Spy>(serdata);
		Assert.AreEqual(spy.NotSecretCode, spy2.NotSecretCode);
	}

	[Test]
	public void DataModels()
	{
		var fred = new Dog("Fred", "beagle");
		var serdata = Serializer.SerializeToString(fred);
		var fred2 = Serializer.DeserializeFromString<Dog>(serdata);

		// make sure we actually use the data model
		Assert.IsTrue(serdata.Contains("Fred:beagle"));

		// make sure serialization is correct
		Assert.AreEqual(fred.Name, fred2.Name);
		Assert.AreEqual(fred.Breed, fred2.Breed);
	}

	private class Car
	{
		public Car(Company manufacturer, string model, int year)
		{
			Manufacturer = manufacturer;
			Model = model;
			Year = year;
		}

		public Company Manufacturer { get; private set; }

		public string Model { get; private set; }

		public int Year { get; private set; }

		public override string ToString() => $"{Year} {Manufacturer} {Model}";
	}

	private class Company
	{
		public Company(string name)
		{
			Name = name;
		}

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	private class Person
	{
		public Person(string name)
		{
			Name = name;
		}

		public string Name { get; set; }

		public Person Partner { get; set; }

		public override string ToString() => $"{Name ?? "Nobody"}, whose partner is {Partner?.Name ?? "nobody"}";
	}

	private class Spy
	{
		public Spy()
		{
			
		}

		private int SecretCode { get; set; }

		public int NotSecretCode => SecretCode;

		public void SetSecretCode(int code) => SecretCode = code;
	}

	private class Dog(string name, string breed)
		: IUsesDataModel<Dog, DogData>
	{
		public string Name { get; set; } = name;

		public string Breed { get; set; } = breed;
		public Type DomainObjectType { get; } = typeof(Dog);
		public Type DataModelType { get; } = typeof(DogData);

		public static Dog FromDataModel(DogData model)
		{
			var strs = model.Data.Split(":");
			return new Dog(strs[0], strs[1]);
		}

		public DogData ToDataModel()
			=> new DogData($"{Name}:{Breed}");

		object IUsesDataModel.ToDataModel()
			=> ToDataModel();
	}

	private class DogData(string data)
		: IDataModel
	{
		public string Data { get; set; } = data;
	}
}