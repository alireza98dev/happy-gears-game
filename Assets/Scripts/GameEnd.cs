using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using TapsellSDK;

public class GameEnd : MonoBehaviour
{
    private static GameEnd instance;
    public GameObject winBoard;
    public GameObject star;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    private static bool starsGiven;
    public static string dbPath;
    private string adZoneId = "5f501aa8d4366c0001e28af7";
    private TapsellAd levelAd;
    private static float lastAdShowing = 0;

    private void Start()
    {
        string currentLevel = SceneManager.GetActiveScene().name;
        if (currentLevel != "Level 01" && currentLevel != "Level 02" && currentLevel != "Level 03" && currentLevel != "Level 04")
        {
            Tapsell.RequestAd(adZoneId, false,
                (TapsellAd ad) => {
                    levelAd = ad;
                },
                (string notAvailable) => { },
                (TapsellError error) => { },
                (string networkError) => { },
                (TapsellAd expired) => { }
            );
        }
        
    }

    void Awake()
    {
        winBoard.SetActive(false);
        starsGiven = false;
        instance = this;

        dbPath = Application.persistentDataPath + "/levels.dat";
        if (LevelsList.levels == null)
            LevelsList.init();
    }

    private IEnumerator giveStars(int starsCount)
    {
        
        star.SetActive(false);
        starsGiven = true;

        if (starsCount >= 1)
        {
            GameObject star1Clone = Instantiate(star);
            star1Clone.transform.position = star1.transform.position;
            star1Clone.transform.SetParent(star1.transform);
            star1Clone.SetActive(true);
            GameSounds.PlayOneShot("star");
            yield return new WaitForSeconds(0.3f);
        }

        if (starsCount >= 2)
        {
            GameObject star2Clone = Instantiate(star);
            star2Clone.transform.position = star2.transform.position;
            star2Clone.transform.SetParent(star2.transform);
            star2Clone.SetActive(true);
            GameSounds.PlayOneShot("star");
            yield return new WaitForSeconds(0.3f);
        }

        if (starsCount >= 3)
        {
            GameObject star3Clone = Instantiate(star);
            star3Clone.transform.position = star3.transform.position;
            star3Clone.transform.SetParent(star3.transform);
            star3Clone.SetActive(true);
            GameSounds.PlayOneShot("star");
            yield return new WaitForSeconds(0.3f);
        }
        float deltaTimeAd = Time.time - lastAdShowing;
        if(levelAd != null && (lastAdShowing == 0 || deltaTimeAd >= 120) )
        {
            Tapsell.ShowAd(levelAd);
            lastAdShowing = Time.time;
        }

    }
    public static void SaveLevel(int starsCount)
    {
        try
        {
            /// Opening db file
            FileStream file;

            if (File.Exists(dbPath))
            {
                file = File.OpenRead(dbPath);
            }
            else
            {
                file = File.Create(dbPath);
            }

            /// Deserialize the levels data
            BinaryFormatter bf = new BinaryFormatter();
            LevelsModel levels;
            try
            {
                levels = bf.Deserialize(file) as LevelsModel;
            }
            catch (Exception)
            {
                levels = new LevelsModel();
            }
            file.Close();

            if (levels.data == null)
                levels.data = new Dictionary<string, LevelData>();


            string currentLevelName = SceneManager.GetActiveScene().name;
            LevelData newData = new LevelData(currentLevelName, starsCount);

            if (levels.data.ContainsKey(currentLevelName))
            {
                if (levels.data[currentLevelName].stars > newData.stars)
                    newData.stars = levels.data[currentLevelName].stars;

                levels.data[currentLevelName] = newData;
            }
            else
            {
                levels.data.Add(currentLevelName, newData);
            }

            file = File.OpenWrite(dbPath);
            bf.Serialize(file, levels);
            file.Close();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    public static void onWin(int stars)
    {
        instance.StartCoroutine(_onWin(stars));
    }
    public static IEnumerator _onWin(int stars)
    {
        SaveLevel(stars);
        MusicController.audioSource.volume = 0.3f ;
        yield return new WaitForSeconds(0.3f);
        MusicController.PlayWinSound();
        yield return new WaitForSeconds(1f);
        MusicController.audioSource.volume = 1f;
        instance.winBoard.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        if (!starsGiven)
            instance.StartCoroutine(instance.giveStars(stars));

        

    }
    public void nextLevel()
    {
        int currentIndex = LevelsList.levels.IndexOf(SceneManager.GetActiveScene().name);
        string nextLevelName = null;

        try
        {
            if (LevelsList.levels.Count > currentIndex + 1)
                nextLevelName = LevelsList.levels[currentIndex + 1];
        }
        catch (Exception) { }

        if (nextLevelName != null)
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            goHome();
        }
    }
    public void goHome()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void tryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
