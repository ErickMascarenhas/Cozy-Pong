using UnityEngine;

public class SongLoader : MonoBehaviour
{
    public GameObject[] ItemsToDisable;
    public GameObject[] ItemsToEnable;

    public void LoadSong()
    {
        for (int i = 0; i < ItemsToDisable.Length; i++)
        {
            ItemsToDisable[i].SetActive(false);
        }
        for (int i = 0;i < ItemsToEnable.Length; i++)
        {
            ItemsToEnable[i].SetActive(true);
        }
    }
}