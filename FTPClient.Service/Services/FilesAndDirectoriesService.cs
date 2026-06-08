using FTPClient.Models;
using FTPClient.Service.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using FTPClient.Models.Models;

namespace FTPClient.Service.Services
{
    public class FilesAndDirectoriesService : IFilesAndDirectoriesService
    {
        private static string settingsFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/userSettings.json";
        private static string profileFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/profile.json";
        public Result SaveUserConfigFile(Profile profile)
        {
            try
            {
                var allProfilesResult = GetUserSettings();
                if (!allProfilesResult.IsSuccess)
                    return Result.Failure([$"Get user settings error: {allProfilesResult.Errors}"]);
                var allProfiles = allProfilesResult.Value;
                var indexOfMyProfile = -1;
                foreach (var p in allProfiles)
                {
                    if(p.Name == profile.Name)
                    {
                        indexOfMyProfile  = allProfiles.IndexOf(p);
                    }
                }
                if (indexOfMyProfile > -1)
                {
                    if (!string.IsNullOrEmpty(profile.ProfileSettings.LocalPath))
                    {
                        allProfiles[indexOfMyProfile].ProfileSettings.LocalPath = profile.ProfileSettings.LocalPath;
                    }
                    var color = profile.ProfileSettings.ProfileColor;
                    if(color != null)
                    {
                        allProfiles[indexOfMyProfile].ProfileSettings.ProfileColor = profile.ProfileSettings.ProfileColor;
                    }
                    string jsonFile = JsonSerializer.Serialize(allProfiles);
                    using (StreamWriter writer = new StreamWriter(settingsFilePath))
                    {
                        writer.Write(jsonFile);
                    }
                }
                else
                {
                    Profile newProfile = new Profile()
                    {
                        Name = profile.Name,
                        ProfileSettings = new ProfileSettings
                        {
                            LocalPath = profile.ProfileSettings.LocalPath,
                            ProfileColor = profile.ProfileSettings.ProfileColor,
                        }
                    };
                    allProfiles.Add(newProfile);

                    string jsonFile = JsonSerializer.Serialize(allProfiles);
                    using (StreamWriter writer = new StreamWriter(settingsFilePath))
                    {
                        writer.Write(jsonFile);
                    }
                }

                return Result.Success();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveUserConfigFile error: {ex}");
                return Result.Failure([$"SaveUserConfigFile error: {ex.Message} - {ex.InnerException}"]);
            }
            
        }
        public Result<Profile> GetUserSettings(string profileName)
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    using FileStream fs = File.Create(settingsFilePath);
                }
                string userSettings = string.Empty;
                using (StreamReader reader = new StreamReader(settingsFilePath))
                {
                    userSettings = reader.ReadToEnd();
                }
                if (string.IsNullOrWhiteSpace(userSettings))
                {
                    return Result.Failure<Profile>(["user settings are empty"]);
                }
                else
                {
                    var settings = JsonSerializer.Deserialize<List<Profile>>(userSettings);
                    foreach (Profile profile in settings)
                    {
                        if (profile.Name == profileName)
                        {
                            return profile != null ? Result.Success(profile) : Result.Failure<Profile>(["Found empty settings object"]);
                        }
                    }
                    return Result.Failure<Profile>(["No user settings found"]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetUserSettings error: {ex}");
                return Result.Failure<Profile>([$"GetUserSettings error: {ex.Message} - {ex.InnerException}"]);
            }
        }
        public Result<List<Profile>> GetUserSettings()
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    Profile newProfile = new Profile()
                    {
                        Name = "Default",
                        ProfileSettings = new ProfileSettings
                        {
                            LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            ProfileColor = new JsonColor
                            {
                                R = 36, G = 39, B = 42
                            },
                        }
                    };
                    SaveUserConfigFile(newProfile);
                }
                
                var userSettings = File.ReadAllText(settingsFilePath);
                var settings = JsonSerializer.Deserialize<List<Profile>>(userSettings);
                if (settings == null || !settings.Any())
                    return Result.Failure<List<Profile>>(["Settings are not found"]);
                
                return Result.Success(settings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetUserSettings error: {ex}");
                return Result.Failure<List<Profile>>([$"GetUserSettings error: {ex.Message} - {ex.InnerException}"]);
            }
        }
        public Result SaveCurrentProfile(string profileName)
        {
            try
            {
                string jsonFile = JsonSerializer.Serialize(profileName);
                File.WriteAllText(profileFilePath, jsonFile);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveCurrentProfile error: {ex}");
                return Result.Failure<List<Profile>>([$"SaveCurrentProfile error: {ex.Message} - {ex.InnerException}"]);
            }
        }
        public Result SaveCurrentProfile()
        {
            try
            {
                string jsonFile = JsonSerializer.Serialize("Default");
                File.WriteAllText(profileFilePath, jsonFile);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService SaveCurrentProfile error: {ex}");
                return Result.Failure<List<Profile>>([$"SaveCurrentProfile error: {ex.Message} - {ex.InnerException}"]);
            }
        }
        public Result<string> GetCurrentProfile()
        {
            try
            {
                if (!File.Exists(profileFilePath))
                {
                    SaveCurrentProfile("Default");
                }
                var currentProfileJson = File.ReadAllText(profileFilePath);
                var currentProfile = JsonSerializer.Deserialize<string>(currentProfileJson);
                if (string.IsNullOrEmpty(currentProfile))
                    return Result.Failure<string>(["Current profile is empty"]);
                return Result.Success(currentProfile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService GetCurrentProfile error: {ex}");
                return Result.Failure<string>([$"GetCurrentProfile error: {ex.Message} - {ex.InnerException}"]);
            }
        }
        public Result AddNewProfile(string newProfileName)
        {
            try
            {
                var profilesResult = GetUserSettings();
                if (!profilesResult.IsSuccess)
                    return Result.Failure([$"GetUserSettings error: {profilesResult.Errors}"]);
                
                var profiles = profilesResult.Value;
                
                bool isAlreadyExists = false;
                foreach(var profile in profiles)
                {
                    if(profile.Name == newProfileName)
                    {
                        isAlreadyExists = true;
                    }
                }

                if (!isAlreadyExists)
                {
                    profiles.Add(new Profile()
                    {
                        Name = newProfileName,
                        ProfileSettings = new ProfileSettings()
                        {
                            LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                        }
                    });
                    string jsonFile = JsonSerializer.Serialize(profiles);
                    File.WriteAllText(settingsFilePath, jsonFile);
                    return Result.Success();
                }
                return Result.Failure(["The profile already exists"]);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService AddNewProfile error: {ex}");
                return Result.Failure([$"AddNewProfile error: {ex.Message} - {ex.InnerException}"]);
            }
        } 
        public Result DeleteProfile(string profileName)
        {
            try
            {
                var profilesResult = GetUserSettings();
                if (!profilesResult.IsSuccess)
                    return Result.Failure([$"GetUserSettings error: {profilesResult.Errors}"]);
                
                var profiles = profilesResult.Value;
                
                var profile = profiles.FirstOrDefault(p => p.Name == profileName);
                int indexOfProfile = profiles.IndexOf(profile);
                profiles.RemoveAt(indexOfProfile);

                string jsonFile = JsonSerializer.Serialize(profiles);
                File.WriteAllText(settingsFilePath, jsonFile);
                return Result.Success();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"FilesAndDirectoriesService DeleteProfile error: {ex}");
                return Result.Failure<List<Profile>>([$"DeleteProfile error: {ex.Message} - {ex.InnerException}"]);
            }
        }
    }
}
