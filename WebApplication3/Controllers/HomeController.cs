using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;


using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Canada(int? id)
        {
            return View();
        }

        //    [HttpPost]
        public string Button_click(object sender, EventArgs e, string province, double hectares, string type, double town, double water, string flayer, int weather)
        {
            string a = "";
            double b = 2;
            double t = 0;

            Gogo go = new Gogo();

            double prov = go.PROV[province];
            go.StartGet(prov, hectares, type, town, water, flayer, weather, ref a, ref b, ref t);

            string result = a.ToString() + ";" + b.ToString() + ";" + t.ToString();
            return result;
        }

        [HttpPost]
        public string Canada()
        {
            return "Спасибо что уцелел";
        }

        public IActionResult CanadaAI()
        {
            return View();
        }

        public IActionResult Calc()
        {
            //   var req = HttpWebRequest.Create("http://7c8d350d-eb8a-4a0c-8839-aacc0e79d5e3.westeurope.azurecontainer.io/score");

            return View();
        }

        public IActionResult Info()
        {
            //          ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contacts()
        {

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


    public class InputData
    {
        [JsonProperty("data")]

        public double[,] data;
    }
    public class Gogo
    {
        public Dictionary<double, Dictionary<string, double>> VARIABLE = new Dictionary<double, Dictionary<string, double>>(14);
        public Dictionary<string, double> PROV = new Dictionary<string, double>(14);
        public Dictionary<double, double> HECT_COEF = new Dictionary<double, double>(14);
        public Dictionary<double, double> HARDWOOD = new Dictionary<double, double>(14);
        public Dictionary<double, double> SOFTWOOD = new Dictionary<double, double>(14);

        public Gogo()
        {
            MyDict.initVARIABLE(ref VARIABLE);
            MyDict.initPROV(ref PROV);
            MyDict.initHECT_COEF(ref HECT_COEF);
            MyDict.initHARDWOOD(ref HARDWOOD);
            MyDict.initSOFTWOOD(ref SOFTWOOD);
        }

        public string StartGet(double prov, double hectares, string type, double town, double water, string flayer, int weather, ref string fix, ref double variable, ref double time) ///main
        {
            double forest = 100 - town - water;
            variable = variableFunc(prov, hectares, town, forest, water);
            time = Convert.ToSingle(get_time(hectares, type, flayer, weather));
            fix = fixedFunc(prov, hectares, ref variable, water, ref time);

            double fixedCost = Convert.ToSingle(fix);
            fixedCost = Math.Round(fixedCost, 0);
            variable = Math.Round(variable, 0);
            fix = fixedCost.ToString();
            return fix;
        }

        public double variableFunc(double prov, double hectares, double town, double forest, double water)
        {
            object[] d = new object[5];
            d[0] = VARIABLE;
            d[1] = PROV;
            d[2] = HECT_COEF;
            d[3] = HARDWOOD;
            d[4] = SOFTWOOD;
            double variable = (VARIABLE[prov]["Town"] * 7.75 * hectares * town / 100) + (VARIABLE[prov]["Road"] * 2.96 + VARIABLE[prov]["Railroad"] * 3.05)
                * hectares * forest / 100 - (water * hectares * 1.25 / 100) * 5.65;
            if (variable <= 10)
                return 0;
            return variable;
        } 

        public string cutStr(string str)
        {
            string res;

            res = str.Remove(0, 15);
            res = res.TrimEnd('}', ']', '"');
            res = res.Replace('.', ',');
            return (res);
        }

        public string fixedFunc(double prov, double hectares, ref double variable, double water, ref double time)
        {
            string scoringUri = "http://ca6b3c5b-f295-4395-bb5b-26b9c80a70eb.westeurope.azurecontainer.io/score";
            string authKey = "";
            double tmp; 
            string resStr;

            // Set the data to be sent to the service.
            // In this case, we are sending two sets of data to be scored.
            InputData payload = new InputData();

            payload.data = new double[,] {
                {
                        DateTime.Now.Year,
                        prov,
                        HARDWOOD[prov],
                        SOFTWOOD[prov],
                        1 - HARDWOOD[prov] - SOFTWOOD[prov],
                        hectares,
                        variable,
                        }
                };



            // Create the HTTP client
            HttpClient client = new HttpClient();
            // Set the auth header. Only needed if the web service requires authentication.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authKey);

            // Make the request
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(scoringUri));
                request.Content = new StringContent(JsonConvert.SerializeObject(payload));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.SendAsync(request).Result;
                // Display the response from the web service
                //      Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                resStr = cutStr(response.Content.ReadAsStringAsync().Result);
                tmp = double.Parse(resStr);

                if (water != 0)
                    tmp = (tmp * (-Math.Log(water / 100)) / (12 * 30)) * time;

                if (tmp <= 10)
                    return ("0");
                return tmp.ToString();
                //   return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
            return null;
        }

        public string get_time(double hectares, string ftype, string flayer, double weather) // ИМПЛЕМЕНТИТЬ ПОГОДУ
        {
            string scoringUri = "http://4a0ece96-bb3f-47fe-8eba-ec81778bca6b.westeurope.azurecontainer.io/score";
            string authKey = "";
            string resStr;


            InputData payload = new InputData();

            int fclass = 0;
            switch (ftype.ToLower())
            {
                case string h when (h == "bh"):
                    fclass = 1;
                    break;
                case string h when (h == "uc"):
                    fclass = 0;
                    break;
                case string h when (h == "to"):
                    fclass = -1;
                    break;
            }

            int fire_type = 0;

            switch (flayer.ToLower())
            {
                case string h when (h == "surface"):
                    fire_type = -1;
                    break;
                case string h when (h == "ground"):
                    fire_type = 0;
                    break;
                case string h when (h == "crown"):
                    fire_type = 1;
                    break;
            }

            

            payload.data = new double[,] {
                {
                        fire_type, // 'Surface': -1, 'Ground': 0, 'Crown': 1                            // AAAAAAAAAAAAAA
                        weather, //'CB Dry': -1, 'CB Wet': 1, 'Clear': 0, 'Cloudy': 2, 'Rainshowers': 3 
                        hectares,
                        fclass, // 'to': -1, 'uc': 0, 'bh': 1 
                        }
                };



            // Create the HTTP client
            HttpClient client = new HttpClient();
            // Set the auth header. Only needed if the web service requires authentication.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authKey);

            // Make the request
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(scoringUri));
                request.Content = new StringContent(JsonConvert.SerializeObject(payload));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.SendAsync(request).Result;
                // Display the response from the web service
                //      Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                resStr = cutStr(response.Content.ReadAsStringAsync().Result);

                double time = Math.Abs(double.Parse(resStr));
                time += 1;
                if (time >= 2){
                    if (hectares != 0)
                    {
                        if (hectares > 1)
                            time = Math.Round(Math.Round(time, 0) * Math.Log10(hectares), 0);
                        else
                            time = Math.Round(Math.Round(time, 0) * hectares, 0);
                        return time.ToString();
                    }
                    else
                        return "0";
                }
                else
                    return time.ToString();
                //   return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
            return null;
        }

    }




    public static class MyDict
    {
        public static void initVARIABLE(ref Dictionary<double, Dictionary<string, double>> d)
        {
            Dictionary<string, double> dInd = new Dictionary<string, double>(3);

            dInd.Add("Railroad", 3.33277787035495);
            dInd.Add("Road", 33.3344442592902);
            dInd.Add("Town", 23.3327778703549);

            d.Add(4, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 1.80036007201436);
            dInd.Add("Road", 18);
            dInd.Add("Town", 16.1996399279856);

            d.Add(2, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 4.4166666666667);
            dInd.Add("Road", 10.4166666666667);
            dInd.Add("Town", 14.5833333333333);

            d.Add(5, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 5.33277787035495);
            dInd.Add("Road", 29.4963129608801);
            dInd.Add("Town", 29.5036870391199);

            d.Add(7, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 8.33277787035495);
            dInd.Add("Road", 54);
            dInd.Add("Town", 27);

            d.Add(11, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 6.54515703831643);
            dInd.Add("Road", 32.7225126130632);
            dInd.Add("Town", 32.7323303486204);

            d.Add(1, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 5.15789473684209);
            dInd.Add("Road", 46.4210526315788);
            dInd.Add("Town", 46.4210526315791);

            d.Add(3, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 2.33277787035495);
            dInd.Add("Road", 24);
            dInd.Add("Town", 24);

            d.Add(8, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 2.33277787035495);
            dInd.Add("Road", 24);
            dInd.Add("Town", 24);

            d.Add(6, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 2.33277787035495);
            dInd.Add("Road", 24);
            dInd.Add("Town", 24);

            d.Add(0, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 2.33277787035495);
            dInd.Add("Road", 24);
            dInd.Add("Town", 24);

            d.Add(9, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 4.33277787035495);
            dInd.Add("Road", 44.5);
            dInd.Add("Town", 44.5);

            d.Add(10, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 2.33277787035495);
            dInd.Add("Road", 24);
            dInd.Add("Town", 24);

            d.Add(12, dInd);

            dInd.Clear();
            dInd.Add("Railroad", 2.33277787035495);
            dInd.Add("Road", 24);
            dInd.Add("Town", 24);

            d.Add(13, dInd);
        }

        public static void initPROV(ref Dictionary<string, double> d)
        {
            d.Add("CN", 0);
            d.Add("ON", 1);
            d.Add("BC", 2);
            d.Add("QC", 3);
            d.Add("AB", 4);
            d.Add("MB", 5);
            d.Add("NS", 6);
            d.Add("NB", 7);
            d.Add("SK", 8);
            d.Add("NL", 9);
            d.Add("YT", 10);
            d.Add("NT", 11);
            d.Add("PE", 12);
            d.Add("NU", 13);
        }

        public static void initANOTER(ref Dictionary<double, double> d, int number)
        {
            if (number == 0)
                initHECT_COEF(ref d);
            if (number == 1)
                initHARDWOOD(ref d);
            if (number == 2)
                initSOFTWOOD(ref d);
        }

        public static void initHECT_COEF(ref Dictionary<double, double> d)
        {
            d.Add(0, 75.940546935);
            d.Add(1, 25.47817458);
            d.Add(2, 39.32252245);
            d.Add(3, 50.07386497);
            d.Add(4, 75.8843514);
            d.Add(5, 21.40350151);
            d.Add(6, 75.940546935);
            d.Add(7, 75.940546935);
            d.Add(8, 7.9709706229999995);
            d.Add(9, 75.940546935);
            d.Add(10, 10.18072289);
            d.Add(11, 15.78476854);
            d.Add(12, 75.940546935);
            d.Add(13, 75.940546935);
        }

        public static void initHARDWOOD(ref Dictionary<double, double> d)
        {
            d.Add(0, 0.07);
            d.Add(1, 0.36);
            d.Add(2, 0.04);
            d.Add(3, 0.38);
            d.Add(4, 0.44);
            d.Add(5, 0.37);
            d.Add(6, 0.28);
            d.Add(7, 0.36);
            d.Add(8, 0.43);
            d.Add(9, 0.03);
            d.Add(10, 0.08);
            d.Add(11, 0.10);
            d.Add(12, 0.35);
            d.Add(13, 0.10);
        }

        public static void initSOFTWOOD(ref Dictionary<double, double> d)
        {
            d.Add(0, 0.03);
            d.Add(1, 0.64);
            d.Add(2, 0.96);
            d.Add(3, 0.62);
            d.Add(4, 0.56);
            d.Add(5, 0.63);
            d.Add(6, 0.72);
            d.Add(7, 0.64);
            d.Add(8, 0.57);
            d.Add(9, 0.97);
            d.Add(10, 0.92);
            d.Add(11, 0.9);
            d.Add(12, 0.65);
            d.Add(13, 0.9);
        }
    }
}




