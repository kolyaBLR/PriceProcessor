using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace PriceProcessor
{
    class ExchangeRates
    {
        public string StrCheckNumber(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!Regex.IsMatch(str[i].ToString(), @"[0-9]"))
                {
                    str = str.Remove(i, 1);
                    i--;
                }
            }
            return str;
        }

        public string data()
        {
            string str = "";
            for (int i = 0; i < DateTime.Today.ToString().Length; i++)
            {
                if (DateTime.Today.ToString()[i] == ' ')
                    break;
                else str += DateTime.Today.ToString()[i];
            }
           return str.Replace('.', '/');
        }
        public string convert()
        {
            try
            {
                WebRequest request = WebRequest.Create("http://www.nbrb.by/Services/XmlExRates.aspx?ondate=" + data());
                WebResponse response = request.GetResponse();   // ждём ответ
                XmlDocument d = new XmlDocument();
                d.Load(response.GetResponseStream());
                XmlNodeList nodeList = d.GetElementsByTagName("DailyExRates");
                foreach (XmlElement element in nodeList)
                {
                    foreach (XmlElement item in element.ChildNodes)
                    {
                        if (item.InnerText.Contains("Доллар США"))
                        {
                            if (StrCheckNumber(item.InnerText.Substring(item.InnerText.Length - 6, 6).Replace('.', ',')).Length == 4)
                               return StrCheckNumber(item.InnerText.Substring(item.InnerText.Length - 6, 6).Replace('.', ',')) + '0';
                            else return StrCheckNumber(item.InnerText.Substring(item.InnerText.Length - 6, 6).Replace('.', ','));
                        }
                    }
                }

                return "Ошибка!";
            }
            catch
            {
                return "Ошибка!";
            }
        }
    }
}
