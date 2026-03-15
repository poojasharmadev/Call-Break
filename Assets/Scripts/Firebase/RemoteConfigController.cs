using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using UnityEngine;
using Common;
using Newtonsoft.Json;

namespace Firebase
{
    public class RemoteConfigController : SingletonHelper<RemoteConfigController>
    {
        [SerializeField] RemoteConfigData[] remoteConfigDatas;
        Dictionary<string, string> _configValues = new Dictionary<string, string>();

        void Start()
        {
            LoadCache();
            FirebaseInitialize.OnFirebaseInitialized += FetchRemoteConfig;
        }

        void OnDestroy()
        {
            FirebaseInitialize.OnFirebaseInitialized -= FetchRemoteConfig;
        }

        async void FetchRemoteConfig()
        {
            try
            {
                var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                await remoteConfig.FetchAsync(TimeSpan.Zero);
                await remoteConfig.ActivateAsync();
                
                // Update values
                foreach (var data in remoteConfigDatas)
                {
                    string value = remoteConfig.GetValue(data.key).StringValue;
                    _configValues[data.key] = value;
                    data.value = value;
                }
                
                SaveCache();
                Debug.Log("Remote Config fetched successfully");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Remote Config fetch failed: {e.Message}");
            }
        }

        void LoadCache()
        {
            if (!PlayerPrefs.HasKey(Constants.RemoteConfigCache)) return;

            try
            {
                string json = PlayerPrefs.GetString(Constants.RemoteConfigCache);
                _configValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                
                foreach (var data in remoteConfigDatas)
                {
                    if (_configValues.TryGetValue(data.key, out string value))
                        data.value = value;
                }
                
                Debug.Log("Loaded cached Remote Config");
            }
            catch (Exception e)
            {
                Debug.LogError($"Cache load failed: {e.Message}");
            }
        }

        void SaveCache()
        {
            string json = JsonConvert.SerializeObject(_configValues);
            PlayerPrefs.SetString(Constants.RemoteConfigCache, json);
            PlayerPrefs.Save();
        }

        public string GetValue(string key)
        {
            foreach (var data in remoteConfigDatas)
            {
                if (data.key == key)
                    return data.value;
            }

            Debug.LogWarning($"Remote config key not found: {key}");
            return string.Empty;
        }
    }

    [Serializable]
    public class RemoteConfigData
    {
        public string key;
        public string value;
    }
}