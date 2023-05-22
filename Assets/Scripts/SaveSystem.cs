using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// Adapted from https://youtu.be/XOjd_qU2Ido (Brackeys)
public static class SaveSystem
{
    public static readonly string savePath = Application.persistentDataPath + "/chess.game";

    public static void SaveGame(bool isBlackTurn, bool isTwoPlayer, List<BasePiece> pieces)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        GameData data = new GameData(isBlackTurn, isTwoPlayer, pieces);

        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("Game saved!");
    }

    public static GameData LoadGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();  // Is there a reason this is instanciated in both methods?
        try
        {
            FileStream stream = new FileStream(savePath, FileMode.Open);
            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();
            Debug.Log("Game loaded!");
            return data;
        } catch (FileNotFoundException)
        {
            Debug.Log("No save game exists");
            return null;
        }
        //finally { if (stream != null) stream.Close(); }
    }
}
