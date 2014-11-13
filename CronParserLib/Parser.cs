using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronParserLib
{
    //秒	 	0-59	 	            , - * /
    //分	 	0-59	 	            , - * /
    //小时	 	0-23	 	            , - * /
    //日期	 	1-31	 	            , - * ? / L W
    //月份	 	1-12 或者 JAN-DEC	, - * /
    //星期	 	1-7 或者 SUN-SAT	 	, - * ? / L #
    //年（可选）留空, 1970-2099	 	, - * /

    public class Parser
    {
        private class CronObject
        {
            public string CronExpression { get; set; }
        }

        private static Dictionary<char, string> cronDictionary = new Dictionary<char, string>(){
                {',',"於"},
                {'*',"每"},
                {'-',"從_至"},
                {'/',"自第_開始每"},
                {'?',""},
                {'L',"倒數第_最後的"},
                {'W',"個工作日"},
                {'#',"第_週的"}
            };

        private static Dictionary<char, string> CronDictionary
        {
            get { return cronDictionary; }
        }

        private static Dictionary<string, int> cronRange = new Dictionary<string, int>(){
                {"s_Start",0},
                {"s_End",59},
                {"m_Start",0},
                {"m_End",59},
                {"h_Start",0},
                {"h_End",23},
                {"d_Start",1},
                {"d_End",31},
                {"M_Start",1},
                {"M_End",12},
                {"w_Start",1},
                {"w_End",7},
                {"y_Start",1970},
                {"y_End",2099}
            };

        private static Dictionary<string, int> CronRange
        {
            get { return cronRange; }
        }

        private static Dictionary<string, string> cronSignLimit = new Dictionary<string, string>(){
                {"s_Sign",", - * /"},
                {"m_Sign",", - * /"},
                {"h_Sign",", - * /"},
                {"d_Sign",", - * / ?"},
                {"M_Sign",", - * /"},
                {"w_Sign",", - * / ?"},
                {"y_Sign",", - * /"},
                {"d_SpecialSign","L W"},
                {"w_SpecialSign","L #"}
            };

        private static Dictionary<string, string> CronSignLimit
        {
            get { return cronSignLimit; }
        }

        private static List<string> monthEn = new List<string> { "", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        private static List<string> MonthEn
        {
            get { return monthEn; }
        }

        private static List<string> monthCh = new List<string> { "", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二" };

        private static List<string> MonthCh
        {
            get { return monthCh; }
        }

        private static List<string> weekEn = new List<string> { "", "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };

        private static List<string> WeekEn
        {
            get { return weekEn; }
        }

        private static List<string> weekCh = new List<string> { "", "日", "一", "二", "三", "四", "五", "六" };

        private static List<string> WeekCh
        {
            get { return weekCh; }
        }

        private static string[] numEn = new string[] { "s", "m", "h", "d", "M", "w", "y" };
        private static string[] numCh = new string[] { "秒", "分", "點", "日", "月", "週", "年" };

        private static string[] NumCh
        {
            get { return numCh; }
            set { numCh = value; }
        }

        private static string[] NumEn
        {
            get { return numEn; }
        }

        /// <summary>
        /// Parser Cron Expression
        /// </summary>
        /// <param name="cron"></param>
        /// <returns></returns>
        public string CronToChineseParser(string cron)
        {
            int cronSplitLength = cron.Split(' ').Length;

            //預設年度為*
            string[] cronSplit = new string[7] { "", "", "", "", "", "", "*" };

            string[] parsingSb = new string[7];

            if (cronSplitLength == 6 || cronSplitLength == 7)
            {
                cron.Split(' ').CopyTo(cronSplit, 0);

                if (!verifyDayOfMonthAndDayOfWeek(cronSplit[3], cronSplit[5]))
                    throw new FormatException("illegal setting!");

                for (int i = cronSplit.Length - 1; i > -1; i--)
                {
                    StringBuilder cronSign = new StringBuilder();

                    foreach (string s in CronSignLimit[NumEn[i] + "_Sign"].Split(' '))
                    {
                        //判斷是否包含符號
                        if (cronSplit[i].Contains(s))
                            cronSign = cronSign.Append(s);
                    }

                    if (cronSign.Length > 1)
                        throw new FormatException("illegal setting!");

                    if (i == 3 || i == 5)
                    {
                        string temp = ProcessSign(i, Convert.ToChar(cronSign.Length == 0 ? "," : cronSign.ToString()), ProcessSpecialSign(i, cronSplit[i], out parsingSb[i]));
                        if (!string.IsNullOrEmpty(temp))
                        {
                            if (parsingSb[i].Contains(cronDictionary['W']))
                            {
                                parsingSb[i] = parsingSb[i].Replace("@@", temp.Replace(NumCh[3], "")) + " ";
                            }
                            else
                                parsingSb[i] = parsingSb[i].Replace("@@", temp) + " ";
                        }
                        else
                            parsingSb[i] = parsingSb[i].Replace("@@", "");
                    }
                    else
                        parsingSb[i] = ProcessSign(i, Convert.ToChar(cronSign.Length == 0 ? "," : cronSign.ToString()), cronSplit[i]) + " ";
                }

                return cronSplit[6] != "*" ? parsingSb[6] : "" + parsingSb[4] + (parsingSb[5].Contains(cronDictionary['#'].Split('_')[1]) ? parsingSb[5].Replace(cronDictionary['*'], "") : parsingSb[5]) + parsingSb[3] + (parsingSb[2].Contains(cronDictionary['*']) ? parsingSb[2].Replace(NumCh[2], "小時") : parsingSb[2]) + parsingSb[1] + parsingSb[0] + "觸發";
            }
            else
                throw new FormatException("illegal setting!");
        }

        private static int transferEnWeekAndEnMonthToNumber(int i, string cronSplit)
        {
            if (i == 4)
                if (MonthEn.Contains(cronSplit))
                    return MonthEn.IndexOf(cronSplit);

            if (i == 5)
                if (WeekEn.Contains(cronSplit))
                    return WeekEn.IndexOf(cronSplit);

            return -1;
        }

        /// <summary>
        /// 驗證是否為合法設定範圍
        /// </summary>
        /// <param name="i">欄位</param>
        /// <param name="cronSplit">驗證文字</param>
        /// <returns>T/F</returns>
        private static bool verifyRange(int i, string cronSplit)
        {
            int cronNum = 0;
            try
            {
                if (i >= 4 && i <= 5)
                {
                    int.TryParse(cronSplit, out cronNum);
                    if (cronNum == 0)
                        cronNum = transferEnWeekAndEnMonthToNumber(i, cronSplit);
                }
                else
                    cronNum = int.Parse(cronSplit);

                if (cronNum >= CronRange[NumEn[i] + "_Start"] && cronNum <= CronRange[NumEn[i] + "_End"])
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                throw new FormatException("illegal setting!");
            }

        }

        /// <summary>
        /// 驗證增量是否為合法設定範圍
        /// </summary>
        /// <param name="i">欄位</param>
        /// <param name="startTime">起始時間</param>
        /// <param name="addTime">增量時間</param>
        /// <returns>T/F</returns>
        private static bool verifyAddRange(int i, string startTime, string addTime)
        {
            int startTimeCronNum = 0;
            int addTimeCronNum = 0;
            try
            {
                addTimeCronNum = int.Parse(addTime);

                if (i >= 4 && i <= 5)
                {
                    int.TryParse(startTime, out startTimeCronNum);
                    if (startTimeCronNum == 0)
                        startTimeCronNum = transferEnWeekAndEnMonthToNumber(i, startTime);
                }
                else
                    startTimeCronNum = int.Parse(startTime);

                if (verifyRange(i, (startTimeCronNum + addTimeCronNum).ToString()))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                throw new FormatException("illegal setting!");
            }


        }

        /// <summary>
        /// 驗證Day Of Month And Day Of Week必定要有一個 '?' 字元
        /// </summary>
        /// <param name="DayOfMonth">DayOfMonth</param>
        /// <param name="DayOfWeek">DayOfWeek</param>
        /// <returns>T/F</returns>
        private static bool verifyDayOfMonthAndDayOfWeek(string DayOfMonth, string DayOfWeek)
        {
            if (Convert.ToInt32(DayOfMonth.Contains('?')) + Convert.ToInt32(DayOfWeek.Contains('?')) == 1)
                return true;

            return false;
        }

        private static string ProcessSign(int i, char Sign, string cronSplit)
        {
            if (Sign == '-')
                if (cronSplit.Split(Sign).Length == 2)
                    if (verifyRange(i, cronSplit.Split(Sign)[0]) && verifyRange(i, cronSplit.Split(Sign)[1]))
                    {
                        string temp = cronSplit;
                        if (i == 4)
                            cronSplit = MonthAndWeekEnTransToCh(MonthEn, MonthCh, temp.Split(Sign)[1], MonthAndWeekEnTransToCh(MonthEn, MonthCh, temp.Split(Sign)[0], cronSplit, Sign), Sign);

                        if (i == 5)
                        {
                            cronSplit = MonthAndWeekEnTransToCh(WeekEn, WeekCh, temp.Split(Sign)[1], MonthAndWeekEnTransToCh(WeekEn, WeekCh, temp.Split(Sign)[0], cronSplit, Sign), Sign);
                            return cronDictionary[Sign].Split('_')[0] + NumCh[i] + cronSplit.Split(Sign)[0] + cronDictionary[Sign].Split('_')[1] + NumCh[i] + cronSplit.Split(Sign)[1];
                        }

                        return cronDictionary[Sign].Split('_')[0] + cronSplit.Split(Sign)[0] + cronDictionary[Sign].Split('_')[1] + cronSplit.Split(Sign)[1] + NumCh[i];
                    }

            if (Sign == ',')
            {
                string temp = cronSplit;
                foreach (string item in temp.Split(Sign))
                {
                    if (verifyRange(i, item))
                    {
                        if (i == 4)
                            cronSplit = MonthAndWeekEnTransToCh(MonthEn, MonthCh, item, cronSplit, Sign);

                        if (i == 5)
                            cronSplit = MonthAndWeekEnTransToCh(WeekEn, WeekCh, item, cronSplit, Sign);
                    }
                    else
                        throw new FormatException("illegal setting!");
                }

                //排序
                cronSplit = SortCronSplit(i, Sign, cronSplit);

                if (i == 5)
                    return cronDictionary['*'] + NumCh[i] + cronSplit;
                else if (i == 4 || i == 6)
                    return cronDictionary[Sign] + cronSplit + NumCh[i];
                else
                    return cronSplit.Replace(",", NumCh[i] + ",") + NumCh[i];
            }

            if (Sign == '*')
                if (cronSplit.Length == 1)
                    return cronDictionary[Sign] + NumCh[i];

            if (Sign == '/')
                if (cronSplit.Split(Sign).Length == 2)
                    if (verifyRange(i, cronSplit.Split(Sign)[0]))
                        if (verifyAddRange(i, cronSplit.Split(Sign)[0], cronSplit.Split(Sign)[1]))
                        {
                            if (i == 4)
                                cronSplit = MonthAndWeekEnTransToCh(MonthEn, MonthCh, cronSplit.Split(Sign)[0], cronSplit, Sign);

                            if (i == 5)
                                cronSplit = MonthAndWeekEnTransToCh(WeekEn, WeekCh, cronSplit.Split(Sign)[0], cronSplit, Sign);

                            return cronDictionary[Sign].Split('_')[0] + cronSplit.Split(Sign)[0] + NumCh[i] + cronDictionary[Sign].Split('_')[1] + cronSplit.Split(Sign)[1] + NumCh[i];
                        }

            if (Sign == '?')
                if (cronSplit.Length == 1)
                    return cronDictionary[Sign];

            if (Sign == 'L')
                if (cronSplit.Length == 1)
                    return cronDictionary[Sign];

            throw new FormatException("illegal setting!");
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="i">欄位</param>
        /// <param name="Sign">分隔符號</param>
        /// <param name="cronSplit">排序字串</param>
        /// <returns>排序結果</returns>
        private static string SortCronSplit(int i, char Sign, string cronSplit)
        {
            List<int> sortCronSplit = new List<int>();
            foreach (string item in cronSplit.Split(Sign))
            {
                if (i == 4)
                    sortCronSplit.Add(MonthCh.IndexOf(item));
                else if (i == 5)
                    sortCronSplit.Add(WeekCh.IndexOf(item));
                else
                    sortCronSplit.Add(int.Parse(item));
            }
            sortCronSplit.Sort();
            StringBuilder tempCronSplit = new StringBuilder();
            for (int x = 0; x < sortCronSplit.Count; x++)
            {
                if (i == 4)
                    tempCronSplit.Append(MonthCh[sortCronSplit[x]] + ",");
                else if (i == 5)
                    tempCronSplit.Append(WeekCh[sortCronSplit[x]] + ",");
                else
                    tempCronSplit.Append(sortCronSplit[x] + ",");
            }
            cronSplit = tempCronSplit.ToString().TrimEnd(',');
            return cronSplit;
        }

        private static string MonthAndWeekEnTransToCh(List<string> enlist, List<string> chlist, string cronEn, string cronSplit, char Sign)
        {
            //包覆符號，以避免複寫至其他段落字串內的相同字串
            string temp = Sign + cronSplit + Sign;
            if (enlist.IndexOf(cronEn) > 0)
                temp = temp.Replace(Sign + cronEn + Sign, Sign + chlist[enlist.IndexOf(cronEn)] + Sign).TrimStart(Sign).TrimEnd(Sign);
            else
                temp = temp.Replace(Sign + cronEn + Sign, Sign + chlist[int.Parse(cronEn)] + Sign).TrimStart(Sign).TrimEnd(Sign);
            return temp;
        }

        private static string ProcessSpecialSign(int i, string cronSplit, out string parsingSb)
        {
            string temp = "";
            if (i == 3)
            {
                if (cronSplit.Contains('W'))
                    temp = "@@" + cronDictionary['W'];
                else
                    temp = "@@";

                if (cronSplit.Contains('L'))
                    temp = cronDictionary['L'].Split('_')[0] + temp;

                cronSplit = cronSplit.Trim(new char[] { 'L', 'W' });
            }

            if (i == 5)
            {
                if (cronSplit.Contains('#'))
                {
                    int cronSharp = 0;
                    int.TryParse(cronSplit.Split('#')[1], out cronSharp);
                    if (cronSharp > 0)
                    {
                        temp = cronDictionary['#'].Split('_')[0] + cronSplit.Split('#')[1] + cronDictionary['#'].Split('_')[1];
                        cronSplit = cronSplit.Replace('#' + cronSplit.Split('#')[1], "");
                    }
                    else
                        throw new FormatException("illegal setting!");
                }

                if (cronSplit.Contains('L'))
                    temp = cronDictionary['L'].Split('_')[1] + temp;

                cronSplit = cronSplit.Trim(new char[] { 'L', '#' });

                temp = temp + "@@";
            }
            parsingSb = temp;

            return cronSplit;
        }
    }
}