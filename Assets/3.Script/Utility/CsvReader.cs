using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions; // 정규표현식 사용을 위해 추가

public class CsvReader
{
    public static List<DialogueData> LoadCsvData(string csvFileName)
    {
        List<DialogueData> dialogueDatas = new List<DialogueData>();

        // Resources 폴더에서 로드
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);

        if (csvFile == null)
        {
            Debug.LogError($"[CsvReader] Resources 폴더에 '{csvFileName}' 파일이 없습니다.");
            return null;
        }

        // 줄바꿈 기호(\r\n, \n, \r)를 기준으로 행 분리
        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1) return dialogueDatas;

        // 헤더 파싱
        string[] headers = lines[0].Split(',');

        // 정규표현식: 쉼표(,)를 기준으로 나누되, 따옴표("") 안의 쉼표는 무시함
        string pattern = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

        for (int i = 1; i < lines.Length; i++)
        {
            // Regex를 사용하여 행 분할
            string[] values = Regex.Split(lines[i], pattern);

            if (values.Length == 0 || values[0] == "") continue; // 빈 줄 무시

            DialogueData data = new DialogueData();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                string header = headers[j].Trim();
                // 엑셀 저장 시 생기는 따옴표(") 제거 및 앞뒤 공백 제거
                string value = values[j].Trim().Trim('"').Replace("\"\"", "\"");

                switch (header)
                {
                    case "ID":
                        int.TryParse(value, out data.ID);
                        break;
                    case "Name":
                        data.Name = value;
                        break;
                    case "Dialogue":
                        data.Dialogue = value;
                        break;
                    case "HasChoice":
                        // 엑셀에서 TRUE/FALSE가 대문자로 올 수 있으므로 대소문자 무시 비교
                        data.HasChoice = bool.TryParse(value, out bool result) ? result : (value.ToUpper() == "TRUE");
                        break;
                    case "AcceptDialogue":
                        data.AcceptDialogue = value;
                        break;
                    case "AcceptID":
                        int.TryParse(value, out data.AcceptID);
                        break;
                    case "RejectDialogue":
                        data.RejectDialogue = value;
                        break;
                    case "RejectID":
                        int.TryParse(value, out data.RejectID);
                        break;
                    case "Favorability":
                        int.TryParse(value, out data.Favorability);
                        break;
                    case "ImagePath":
                        data.ImagePath = value;
                        break;
                }
            }
            dialogueDatas.Add(data);
        }

        return dialogueDatas;
    }
}