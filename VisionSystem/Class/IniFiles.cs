using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;


public class IniFiles
{
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);  //ini 파일 쓰기

    //ini 파일 읽기
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath); //ini 파일 읽기


    public void WriteIniFile(string strSection, string strKey, string strValue, string strPath, string strFileNmae)
    {
        DirectoryInfo dr = new DirectoryInfo(strPath);
        if (!dr.Exists)
            dr.Create();

        WritePrivateProfileString(strSection, strKey, strValue, strPath + "\\" + strFileNmae);
    }

    public string ReadIniFile(string strSection, string strKey, string strPath, string strFileNmae)
    {
        StringBuilder sb = new StringBuilder(255);
        GetPrivateProfileString(strSection, strKey, "", sb, sb.Capacity, strPath + "\\" + strFileNmae);

        return sb.ToString();
    }
}
