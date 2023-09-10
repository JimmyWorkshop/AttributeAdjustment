using HarmonyLib;
using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace AttributeAdjustment
{
	class AttributeModule : MBSubModuleBase
	{
		public const string Id = "adjustment.attribute.patch";
		public const string DisplayName = "Attribute Adjustment";

		public Harmony Harmony { get; private set; }

		public static int LevelsPerAttribute = 4;

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			Harmony = new Harmony(Id);
		}

		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			base.OnBeforeInitialModuleScreenSetAsRoot();
			var settings = BaseSettingsBuilder.Create(Id, DisplayName)
				.SetFormat("xml")
				.SetFolderName(nameof(AttributeModule))
				.CreateGroup("General", builder => builder
					.AddInteger("LevelsPerAttribute", "Levels Per Attribute", 1, 8, new ProxyRef<int>(() => LevelsPerAttribute, SetOptionAttribute), property => property
						.SetRequireRestart(false)
						.SetHintText("Number of levels required for each attribute point.")));
			var global = settings.BuildAsGlobal();
			global.Register();
			InformationManager.DisplayMessage(new InformationMessage($"{DisplayName} Loaded", Colors.Green));
		}

		public void SetOptionAttribute(int level)
		{
			LevelsPerAttribute = level;
			PatchAttributes();
		}

		public void PatchAttributes()
		{
			var mOriginal = AccessTools.PropertyGetter(typeof(DefaultCharacterDevelopmentModel), "LevelsPerAttributePoint");
			var postfix = typeof(AttributeModule).GetMethod(nameof(PostFix));
			Harmony.Patch(mOriginal, postfix: new HarmonyMethod(postfix));
		}

		public static void PostFix(ref int __result)
		{
			__result = LevelsPerAttribute;
		}
	}
}
