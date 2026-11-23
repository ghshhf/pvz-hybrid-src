using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

public sealed class XWModCapabilities
{
	public const string WidePlantFootprintTransaction = "wide-plant-footprint-transaction-v1";

	public const string WidePlantSurroundTransaction = "wide-plant-surround-transaction-v1";

	public const string WidePlantSupportRuntime = "wide-plant-support-runtime-v1";

	public const string WidePlantColumnPlacement = "wide-plant-column-placement-v1";

	public const string CharacterComponentPhysics = "character-component-physics-v1";

	public const string CharacterComponentAttachmentTransaction = "character-component-attachment-transaction-v1";

	public const string ProjectileSourceAttribution = "projectile-source-attribution-v1";

	public const string SunCollectionAttribution = "sun-collection-attribution-v1";

	public const string CharacterHandleQuery = "character-handle-query-v1";

	public const string DamageEntryPosition = "damage-entry-position-v1";

	public const string GargantuarSmashControl = "gargantuar-smash-control-v1";

	public const string SunRewardReceipt = "sun-reward-receipt-v1";

	public const string ZombieWaterlineVisuals = "zombie-waterline-visuals-v1";

	public const string SunSpendReceipt = "sun-spend-receipt-v1";

	public const string BattleEntryAbort = "battle-entry-abort-v1";

	public const string CameraFollowClaim = "camera-follow-claim-v1";

	public const string BattleSettlementCompletion = "battle-settlement-completion-v1";

	public const string PrivatePackageAssets = "private-package-assets-v1";

	public const string MapSceneRoot = "map-scene-root-v1";

	public const string NativeCharacterSceneDispatch = "native-character-scene-dispatch-v1";

	public const string LevelCatalogPresentation = "level-catalog-presentation-v1";

	public const string UpdateNotifications = "update-notifications-v1";

	public const string UpdateCheckStatus = "update-check-status-v1";

	public const string LoadingPresentation = "loading-presentation-v1";

	private readonly IReadOnlyDictionary<string, XWModCapability> _items;

	public IReadOnlyDictionary<string, XWModCapability> Items => _items;

	public XWModCapabilities(IEnumerable<XWModCapability> capabilities)
	{
		if (capabilities == null)
		{
			throw new ArgumentNullException("capabilities");
		}
		_items = new ReadOnlyDictionary<string, XWModCapability>(capabilities.ToDictionary<XWModCapability, string>((XWModCapability x) => x.Id, StringComparer.Ordinal));
	}

	public bool Supports(string id)
	{
		if (_items.TryGetValue(id, out var value))
		{
			return value.Supported;
		}
		return false;
	}

	public bool IsVerified(string id)
	{
		if (_items.TryGetValue(id, out var value))
		{
			return value.VerifiedOnCurrentPlatform;
		}
		return false;
	}

	public static XWModCapabilities Create(bool isolatedAssembliesSupported, bool isolatedAssembliesVerified = false, IEnumerable<string> installedHookPoints = null, bool hooksVerified = false)
	{
		List<XWModCapability> list = new List<XWModCapability>
		{
			new XWModCapability("wide-plant-surround-transaction-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入多格外壳原子种植、严格居中和替换退款；平台验收结果另行记录。"),
			new XWModCapability("wide-plant-footprint-transaction-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入多格底座原子种植；平台验收结果另行记录。"),
			new XWModCapability("wide-plant-support-runtime-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入宽底座接触选敌、逐格承载预览和实体退款；平台验收结果另行记录。"),
			new XWModCapability("wide-plant-column-placement-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入柱子模式宽底座逐行预览与整批种植、退款和单次卡片结算；平台验收结果另行记录。"),
			new XWModCapability("loading-presentation-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已提供分阶段启动与宿主驱动的加载界面；平台验收结果见宿主文档。"),
			new XWModCapability("update-notifications-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已提供本地更新通知上报、汇总弹窗与管理入口；平台验收结果见宿主文档。"),
			new XWModCapability("update-check-status-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已支持无更新结果上报及自主检查优先于官网结果；平台验收结果见宿主文档。"),
			new XWModCapability("level-catalog-presentation-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已支持目录标题与直接纹理；平台验收结果见宿主文档。"),
			new XWModCapability("native-character-scene-dispatch-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已按场景声明分流原生与伴随角色；平台实机验证尚未记录。"),
			new XWModCapability("map-scene-root-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已提供可直接实例化的通用地图场景根；平台实机验证尚未记录。"),
			new XWModCapability("private-package-assets-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入包内私有原生资源声明；平台实机验证尚未记录。"),
			new XWModCapability("battle-settlement-completion-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入自定义 Process 正式结算完成入口；平台实机验证尚未记录。"),
			new XWModCapability("camera-follow-claim-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入按token归属的相机跟随；平台实机验证尚未记录。"),
			new XWModCapability("battle-entry-abort-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入Process入场终止检查；平台实机验证尚未记录。"),
			new XWModCapability("sun-spend-receipt-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入精确预留、战斗绑定与不可重入的阳光回滚；平台验收结果见宿主文档。"),
			new XWModCapability("zombie-waterline-visuals-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已开放僵尸水线图层配置；平台实机验证尚未记录。"),
			new XWModCapability("sun-reward-receipt-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入单次奖励的实际到账回执；平台验收结果见宿主文档。"),
			new XWModCapability("gargantuar-smash-control-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入巨人实例砸击倍率与世界空间单体目标模式；平台验收结果见宿主文档。"),
			new XWModCapability("damage-entry-position-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入伤害入口目标坐标快照；平台验收结果见宿主文档。"),
			new XWModCapability("character-handle-query-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入已有角色的战斗身份查询；平台验收结果见宿主文档。"),
			new XWModCapability("sun-collection-attribution-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入阳光来源与实际到账归因；平台验收结果见宿主文档。"),
			new XWModCapability("projectile-source-attribution-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入弹丸来源与最终伤害归因；平台验收结果见宿主文档。"),
			new XWModCapability("character-component-physics-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入组件物理调度；平台验收结果见宿主文档。"),
			new XWModCapability("character-component-attachment-transaction-v1", Supported: true, VerifiedOnCurrentPlatform: false, "已接入组件挂载事务；平台验收结果见宿主文档。"),
			new XWModCapability("isolated-assemblies-v1", isolatedAssembliesSupported, isolatedAssembliesVerified, isolatedAssembliesVerified ? "已取得当前平台验证证据。" : "当前平台实机验证尚未记录。")
		};
		HashSet<string> hashSet = new HashSet<string>(installedHookPoints ?? Array.Empty<string>(), StringComparer.Ordinal);
		foreach (string allId in XWModHookPoints.AllIds)
		{
			list.Add(new XWModCapability(allId, hashSet.Contains(allId), hooksVerified && hashSet.Contains(allId), hashSet.Contains(allId) ? "已接入宿主正式调用点。" : "宿主尚未接入此调用点。"));
		}
		foreach (XWModExtensionDescriptor item in XWModExtensionCatalog.Installed)
		{
			list.Add(new XWModCapability(item.Id, Supported: true, VerifiedOnCurrentPlatform: false, $"已接入正式调用点；执行端 {item.Authority}；当前平台实机验证尚未记录。"));
		}
		return new XWModCapabilities(list);
	}
}
