using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System.Net;
using System.Text;

struct Pair
{
    public int Score { get; set; }
    public int Level { get; set; }

    public Pair(int Score, int Level)
    {
        this.Score = Score;
        this.Level = Level;
    }
};

[Serializable]
public class ScoreDTO
{
    public int scoreBoardId;
    public int levelNumber;
    public string name;
    public int score;
}

public class LocalDatabase : MonoBehaviour
{
    private string conn, sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    private IDataReader reader;
    public InputField UserName;
    [SerializeField]
    public Text Score;
    public Text Level;
    [SerializeField]
    public InputField ngrokId;
    public string ngrokDefault;
    public Text Scores;

    int scoreNum = 20000;
    [SerializeField]
    GameObject SaveButton;
    [SerializeField]
    GameObject RefreshButton;
    private bool beenHere = false;
    string DatabaseName = "Scores.s3db";
    string levelText;
    string scoreText;
    string playerName;
    // Start is called before the first frame update
    void Start()
    {

        if (SaveButton != null)
            SaveButton.GetComponent<Button>().onClick.AddListener(() => { insert_function(Level.text[Level.text.Length - 1].ToString(), UserName.text, scoreNum.ToString()); });
        if (RefreshButton != null)
            RefreshButton.GetComponent<Button>().onClick.AddListener(() => { RefreshScores(); });
        //Application database Path android
        string filepath = Application.persistentDataPath + "/" + DatabaseName;
        conn = "URI=file:" + filepath;

        if (!File.Exists(filepath))
        {
            // If not found on android will create Tables and database

            Debug.LogWarning("File \"" + filepath + "\" does not exist. Attempting to create from \"" +
                             Application.persistentDataPath + "!/assets/Scores");



            // UNITY_ANDROID
            WWW loadDB = new WWW("jar:file://" + Application.dataPath + "!/assets/Scores.s3db");
            while (!loadDB.isDone) { }
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDB.bytes);

            Debug.Log("Stablishing connection to: " + conn);
            dbconn = new SqliteConnection(conn);
            dbconn.Open();

            string[] query = {
            "CREATE TABLE LEVELS (ID INTEGER PRIMARY KEY, LevelNumber INTEGER);",
             "CREATE TABLE SCORES (ID INTEGER PRIMARY KEY, LevelNumber INTEGER REFERENCES LEVELS(ID), UserName varchar(20), Score INTEGER);",
             "INSERT INTO LEVELS (LevelNumber) VALUES (1)",
             "INSERT INTO LEVELS (LevelNumber) VALUES (2)",
             "INSERT INTO LEVELS (LevelNumber) VALUES (3)",
             "INSERT INTO LEVELS (LevelNumber) VALUES (4)",
             "INSERT INTO LEVELS (LevelNumber) VALUES (5)",
             "INSERT INTO SCORES (UserName,LevelNumber, Score) VALUES (\"Marcin\",1, 1)",
             "INSERT INTO SCORES (UserName,LevelNumber, Score) VALUES (\"Kaja\",1,2)",
             "INSERT INTO SCORES (UserName,LevelNumber, Score) VALUES (\"Zbych\",1,3)",
             "INSERT INTO SCORES (UserName,LevelNumber, Score) VALUES (\"Karol\",2,4)"
            };
            foreach (var command in query)
            {
                try
                {
                    dbcmd = dbconn.CreateCommand(); // create empty command
                    dbcmd.CommandText = command; // fill the command
                    var rows = dbcmd.ExecuteNonQuery(); // execute command which returns a reader
                    Debug.Log(rows);
                }
                catch (Exception e)
                {

                    Debug.Log(e);

                }
            }
            dbconn.Close();


        }


        if (Level)
            reader_function_level();
        else
            reader_function_all();
    }

    //Insert To Database
    private void insert_function(string Level, string UserName, string Score)
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            dbcmd = dbconn.CreateCommand();
            sqlQuery = string.Format("insert into SCORES (LevelNumber, UserName, Score) values (\"{0}\",\"{1}\",\"{2}\")", Level, UserName, Score);// table name
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }
        Scores.text = "";
        Debug.Log("Insert Done  ");

        reader_function_all();
    }
    //Read All Data For To Database
    private void reader_function_all()
    {

        // int idreaders ;
        string Namereaders, Scorereaders, Levelreaders;
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            IDbCommand dbcmd = dbconn.CreateCommand();
            string sqlQuery = "SELECT  UserName, LevelNumber, Score " + "FROM SCORES";// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            Dictionary<string, Pair> scores = new Dictionary<string, Pair>();
            while (reader.Read())
            {
                // idreaders = reader.GetString(1);
                Namereaders = reader.GetString(0);
                Levelreaders = reader.GetValue(1).ToString();
                Scorereaders = reader.GetValue(2).ToString();


                scores.Add(Namereaders, new Pair(int.Parse(Scorereaders), int.Parse(Levelreaders)));
            }


            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            var sortedDict = scores.OrderBy(p => p.Value.Level).ThenByDescending(p => p.Value.Score);
            int i = 1;
            foreach (var row in sortedDict)
            {
                Scores.text += $"{i}. {row.Key} - {levelText}: {row.Value.Level} {scoreText}: {row.Value.Score}\n";
                i++;
            }
            //       dbconn = null;

        }
    }

    public void RefreshScores()
    {
        if (ngrokId.text != null && ngrokId.text != "")
        {
            ngrokDefault = ngrokId.text;
        }
        List<ScoreDTO> requestScores = new List<ScoreDTO>();

        string Namereaders, Scorereaders, Levelreaders;
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            IDbCommand dbcmd = dbconn.CreateCommand();
            string sqlQuery = "SELECT  UserName, LevelNumber, Score " + "FROM SCORES";// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            Dictionary<string, Pair> scores = new Dictionary<string, Pair>();
            while (reader.Read())
            {
                // idreaders = reader.GetString(1);
                Namereaders = reader.GetString(0).ToString();
                Levelreaders = reader.GetValue(1).ToString();
                Scorereaders = reader.GetValue(2).ToString();


                requestScores.Add(new ScoreDTO() { scoreBoardId = 0, name = Namereaders, levelNumber = int.Parse(Levelreaders), score = int.Parse(Scorereaders) });
            }


            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
        }
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("[ ");
        foreach (ScoreDTO score in requestScores)
        {
            var json = JsonUtility.ToJson(score);
            stringBuilder.Append(json).Append(",");
            //stringBuilder.Append(json.ToString().Replace("\\", "")).Append(",");

        }
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        stringBuilder.AppendLine("]");
        byte[] byteArray = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ngrokDefault + "/API/GameController/RefreshLevels");
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = byteArray.Length;
        using var reqStream = request.GetRequestStream();
        reqStream.Write(byteArray, 0, byteArray.Length);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader StrReader = new StreamReader(response.GetResponseStream());
        string jsonResponse = StrReader.ReadToEnd();
        requestScores = new List<ScoreDTO>();
        var jsonString = jsonResponse.Replace('[', ' ').Replace(']', ' ').Split('}');
        foreach (var score in jsonString)
        {
            var scoreForEach = score;
            if (score[0] == ',')
                scoreForEach = score.Remove(0, 1);
            if (scoreForEach == "" || scoreForEach.Length == 1) break;
            requestScores.Add(JsonUtility.FromJson<ScoreDTO>(scoreForEach + "}"));
        }
        ;

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            foreach (var score in requestScores)
            {
                dbcmd = dbconn.CreateCommand();
                sqlQuery = string.Format("insert into SCORES (LevelNumber, UserName, Score) values (\"{0}\",\"{1}\",\"{2}\")", score.levelNumber, score.name, score.score);// table name
                dbcmd.CommandText = sqlQuery;
                dbcmd.ExecuteScalar();
            }

            dbconn.Close();
        }
        Scores.text = "";
        Debug.Log("Insert Done  ");

        reader_function_all();
    }
    private void reader_function_level()
    {
        if (!SaveButton.active)
            return;
        if (beenHere)
            return;
        beenHere = true;
        Scores.text = "";

        // int idreaders ;
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open(); //Open connection to the database.
            IDbCommand dbcmd = dbconn.CreateCommand();
            string sqlQuery = "SELECT  UserName, Score, LevelNumber " + "FROM SCORES " + "WHERE LevelNumber=" + Level.text[Level.text.Length - 1];// table name
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            Dictionary<string, int> scores = new Dictionary<string, int>();
            while (reader.Read())
            {
                // idreaders = reader.GetString(1);
                scores.Add(reader.GetString(0), reader.GetInt32(1));

                // Scores.text += Namereaders + " - " + Scorereaders + "\n";
                //Debug.Log(" name =" + Namereaders + "Address=" + Scorereaders);
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            if (Score)
                scores.Add(playerName, scoreNum);
            var sortedDict = scores.OrderByDescending(p => p.Value);
            int i = 1;
            foreach (var row in sortedDict)
            {
                Scores.text += $"{i}. {row.Key} - {scoreText}: {row.Value}\n";
                i++;
            }
            //       dbconn = null;

        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Scores.text == "EN" || Scores.text == "PL")
        {
            if (Scores.text == "EN")
            {
                levelText = "Level";
                scoreText = "Scored";
                playerName = "YOU";
            }
            else
            {
                levelText = "Poziom";
                playerName = "TY";
                scoreText = "Wynik";
            }
            Scores.text = "";
            reader_function_all();
        }
        if (SaveButton)
        {
            scoreNum = int.Parse(Score.text.ToString());
            reader_function_level();

        }
    }
}


