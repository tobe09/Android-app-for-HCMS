using System;
using System.Collections.Generic;

namespace PeoplePlusMobile
{
    class CommonMethodsClass
    {
        public DateTime? ConvertStringToDate(string dateStr)
        {
            try
            {
                int[] dateArr = Array.ConvertAll(dateStr.Split('/'), int.Parse);
                int year = dateArr[2];
                int month = dateArr[1];
                int day = dateArr[0];
                DateTime date = new DateTime(year, month, day);
                return date;
            }
            catch
            {
                return null;
            }
        }

        public string ConvertDateToString(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            return day + "/" + month + "/" + year;
        }

        //NOT USED
        public int GetWorkDays(string strFromDate, string strToDate, dynamic workDaysList, dynamic recHolsList, dynamic nonRecHolsList, dynamic holsPayableList)
        {
            int noOfDays, intRecHols = 0, intNonRecHols = 0, workDays = 0;

            DateTime fromDate = DateTime.Parse(strFromDate);
            DateTime toDate = DateTime.Parse(strToDate);

            noOfDays = (toDate - fromDate).Days;
            object[] workDaysArray, recHolArray, nonRecHolsArray;

            //get working days in a week
            if (workDaysList.Count > 0)
            {
                workDaysArray = new object[workDaysList.Count];
                for (int i = 0; i < workDaysList.Count; i++)
                {
                    workDaysArray[i] = workDaysList[i]["WorkDays"];
                }
            }
            else
            {
                workDaysArray = new object[5] { 1, 2, 3, 4, 5 };
            }

            //recurring public holidays
            if (recHolsList.Count > 0)
            {
                recHolArray = new object[recHolsList.Count];
                for (int i = 0; i < recHolsList.Count; i++)
                {
                    recHolArray[i] = recHolsList[i]["RecHols"];
                }
            }
            else
            {
                recHolArray = new object[1] { new DateTime() };
            }

            //non-recurring holidays
            if (nonRecHolsList.Count > 0)
            {
                nonRecHolsArray = new object[nonRecHolsList.Count];
                for (int i = 0; i < nonRecHolsList.Count; i++)
                {
                    nonRecHolsArray[i] = nonRecHolsList[i]["NonRecHols"];
                }
            }
            else
            {
                nonRecHolsArray = new object[1] { new DateTime() };
            }

            Dictionary<DayOfWeek, int> weekDayList = new Dictionary<DayOfWeek, int>();
            weekDayList.Add(DayOfWeek.Monday, 1);
            weekDayList.Add(DayOfWeek.Tuesday, 2);
            weekDayList.Add(DayOfWeek.Wednesday, 3);
            weekDayList.Add(DayOfWeek.Thursday, 4);
            weekDayList.Add(DayOfWeek.Friday, 5);
            weekDayList.Add(DayOfWeek.Saturday, 6);
            weekDayList.Add(DayOfWeek.Sunday, 7);

            for (int i = 0; i < noOfDays; i++)
            {
                DateTime iDate = fromDate.Add(new TimeSpan(i, 0, 0, 0));
                DayOfWeek dayOfWeek = iDate.DayOfWeek;

                for (int j = 0; j < workDaysArray.Length; j++)
                {
                    if (int.Parse(workDaysArray[j].ToString()) == weekDayList[dayOfWeek])
                    {
                        //check date from db and convert appropraitely
                        string strDay = iDate.ToString().Substring(0, 6).Replace('/', ' ').ToUpper();

                        //for recurring holidays count
                        for (int k = 0; k < recHolArray.Length; k++)
                        {
                            if ((string)recHolArray[k] == strDay)
                            {
                                intRecHols++;
                                break;
                            }
                        }

                        //for non-recurring holidays count
                        for (int k = 0; k < nonRecHolsArray.Length; k++)
                        {
                            if (nonRecHolsArray[k].ToString().Substring(0, 6).Replace('/', ' ').ToUpper() == strDay)
                            {
                                intNonRecHols++;
                                break;
                            }
                        }
                        workDays++;
                        break;
                    }
                }
            }

            //check if holidays are payable days
            string holsPayable = "N";               //DEFAULT STATUS 

            if (holsPayableList.Count > 0)
            {
                holsPayable = holsPayableList[0]["Value"];
            }

            if (holsPayable != "Y")
            {
                workDays -= (intRecHols + intNonRecHols);
            }

            return workDays;
        }
    }
}