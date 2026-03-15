using UnityEngine;

namespace Common
{
    public class Constants
    {
        [Header("Local Data Keys")]
        public static readonly string LoginDataKey = "DeviceLoginData";
        public static readonly string ProfileDataKey = "ProfileDataKey";
        public static readonly string StatsDataKey = "StatsDataKey";

        [Header("Key of Firebase Remote Config")]
        public static readonly string ShopItemDataKey = "ShopItemData";
        public static readonly string GiftDataKey = "GiftTimerData";
        public static readonly string InventoryDataKey = "InventoryItemData";
        public static readonly string AdRuleKey = "AdRule";

        [Header("Key of PlayerPrefs")]
        public static readonly string RemoteConfigCache = "RemoteConfigCache";
        public static readonly string InventoryData = "InventoryData";
    }
}
