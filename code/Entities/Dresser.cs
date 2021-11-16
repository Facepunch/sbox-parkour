using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Parkour
{
	class Dresser : EntityComponent<AnimEntity>
	{

		private Clothing.Container clothing = new();

		public void Clear()
		{
			Host.AssertServer();

			clothing?.ClearEntities();
		}

		public void DressFromRandom()
		{
			Host.AssertServer();

			if ( Entity.GetModelName() != "models/citizen/citizen.vmdl" )
			{
				Log.Error( "Putting clothes on wrong model" );
				return;
			}

			clothing.ClearEntities();

			foreach ( var kvp in allClothes )
			{
				var part = new Clothing()
				{
					Category = kvp.Key,
					Model = Rand.FromArray( kvp.Value )
				};
				clothing.Clothing.Add( part );
			}

			clothing.DressEntity( Entity );
		}

		public void DressFromAvatar()
		{
			Host.AssertServer();

			if ( Entity.GetModelName() != "models/citizen/citizen.vmdl" )
			{
				Log.Error( "Putting clothes on wrong model" );
				return;
			}

			if( Entity.Client == null )
			{
				Log.Error( "Dressing avatar when client is null" );
				return;
			}

			clothing.LoadFromClient( Entity.Client );
			clothing.DressEntity( Entity );
		}

		private static Dictionary<Clothing.ClothingCategory, string[]> allClothes = new()
		{
			{
				Clothing.ClothingCategory.Bottoms,
				new[]
				{
					"models/citizen_clothes/trousers/trousers.jeans.vmdl",
					"models/citizen_clothes/trousers/trousers.lab.vmdl",
					"models/citizen_clothes/trousers/trousers.police.vmdl",
					"models/citizen_clothes/trousers/trousers.smart.vmdl",
					"models/citizen_clothes/trousers/trousers.smarttan.vmdl",
					"models/citizen_clothes/trousers/trousers_tracksuitblue.vmdl",
					"models/citizen_clothes/trousers/trousers_tracksuit.vmdl",
					"models/citizen_clothes/trousers/smarttrousers/smarttrousers.vmdl",
					"models/citizen_clothes/shoes/shorts.cargo.vmdl",
				}
			},
			{
				Clothing.ClothingCategory.Tops,
				new[]
				{
					"models/citizen_clothes/jacket/labcoat.vmdl",
					"models/citizen_clothes/jacket/jacket.red.vmdl",
					"models/citizen_clothes/jacket/jacket.tuxedo.vmdl",
					"models/citizen_clothes/jacket/jacket_heavy.vmdl",
				}
			},
			{
				Clothing.ClothingCategory.Footwear,
				new[]
				{
					"models/citizen_clothes/shoes/trainers.vmdl",
					"models/citizen_clothes/shoes/shoes.workboots.vmdl"
				}
			},
			{
				Clothing.ClothingCategory.Hat,
				new[]
				{
					"models/citizen_clothes/hat/hat_hardhat.vmdl",
					"models/citizen_clothes/hat/hat_woolly.vmdl",
					"models/citizen_clothes/hat/hat_securityhelmet.vmdl",
					"models/citizen_clothes/hair/hair_malestyle02.vmdl",
					"models/citizen_clothes/hair/hair_femalebun.black.vmdl",
					"models/citizen_clothes/hat/hat_beret.red.vmdl",
					"models/citizen_clothes/hat/hat.tophat.vmdl",
					"models/citizen_clothes/hat/hat_beret.black.vmdl",
					"models/citizen_clothes/hat/hat_cap.vmdl",
					"models/citizen_clothes/hat/hat_leathercap.vmdl",
					"models/citizen_clothes/hat/hat_leathercapnobadge.vmdl",
					"models/citizen_clothes/hat/hat_securityhelmetnostrap.vmdl",
					"models/citizen_clothes/hat/hat_service.vmdl",
					"models/citizen_clothes/hat/hat_uniform.police.vmdl",
					"models/citizen_clothes/hat/hat_woollybobble.vmdl",
				}
			},
		};

	}
}
