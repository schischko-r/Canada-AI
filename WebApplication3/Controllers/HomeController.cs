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
        public string Button_click(object sender, EventArgs e, string province, double hectares, double  town, double water)
        {
            string a = "";
            double b = 2;
   //         string res_;
            string res;
        //    double res_double;
            Gogo go = new Gogo();

            double prov = go.PROV[province];
            go.StartGet(prov, hectares, town, water, ref a, ref b);
            //       res = res_.Remove(0, 15);
            //     res = res.TrimEnd('}', ']', '"');
            //   res_double = double.Parse(res);
            string result = a.ToString() + ";" +  b.ToString();
            return result;
            //   return View();
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
        // The service used by this example expects an array containing
        //   one or more arrays of doubles
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

        public string StartGet(double prov, double hectares, double town, double water, ref string fix, ref double variable) ///main
        {
            double forest = 100 - town - water;
            variable = variableFunc(prov, hectares, town, forest, water);///double var
            fix = fixedFunc(prov, hectares, ref variable, water);

            double fixedCost = Convert.ToSingle(fix);
            fixedCost =  Math.Round(fixedCost, 0);
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

        public string fixedFunc(double prov, double hectares, ref double variable, double water)
        {
            string scoringUri = "http://7c8d350d-eb8a-4a0c-8839-aacc0e79d5e3.westeurope.azurecontainer.io/score";
            string authKey = "";
            double tmp;
            string resStr;

            // Set the data to be sent to the service.
            // In this case, we are sending two sets of data to be scored.
            InputData payload = new InputData();

            payload.data = new double[,] {
                {
                        prov,
                        HARDWOOD[prov],
                        SOFTWOOD[prov],
                        1 - HARDWOOD[prov] - SOFTWOOD[prov],
                        hectares,
                        variable,
                        }
                };


            //    if water != 0:
            //    fixed = (fixed * hectares / d.HECT_COEF[prov])*(-m.log(water / 100)) / 2
            //  else:
            //   fixed = (fixed * hectares / d.HECT_COEF[prov])
            // if fixed <= 10:
            //   return 0
            // return fixed


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
                    tmp = tmp  * (-Math.Log(water / 100));

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
            d.Add("Canada", 0);
            d.Add("Ontario", 1);
            d.Add("British Columbia", 2);
            d.Add("Quebec", 3);
            d.Add("Alberta", 4);
            d.Add("Manitoba", 5);
            d.Add("Nova Scotia", 6);
            d.Add("New Brunswick", 7);
            d.Add("Saskatchewan", 8);
            d.Add("Newfoundland and Labrador", 9);
            d.Add("Yukon", 10);
            d.Add("Northwest Territories", 11);
            d.Add("Prince Edward Island", 12);
            d.Add("Nunavut", 13);
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





    /*       def variable(prov, hectares, town, forest, water):
   d = dictmain()
   variable = (d.variable[prov]['Town']*7.75*hectares* town/100 + (d.variable[prov]['Road']*2.96 + d.variable[prov]['Railroad']*3.05)*hectares* forest/100 - water* hectares*1.25/100)*8.65
   if variable <=10:
     return 0
   return variable*/
    /*
    def fixedC(prov, hectares, extra, water) :
        scoring_uri = 'http://7c8d350d-eb8a-4a0c-8839-aacc0e79d5e3.westeurope.azurecontainer.io/score'
        key = ''

        d = dictmain()

        data = {"data":
              [
                  [
                    prov,                                             # КОДИФИКАЦИЯ ПРОВИНЦИИ
                    d.HARDWOOD[prov],                                 # HARDWOOD
                    d.SOFTWOOD[prov],                                 # SOFTWOOD
                    1 - d.HARDWOOD[prov] - d.SOFTWOOD[prov],          # UNDEFIGNED
                    hectares,                                         # HECTARES BURNED
                    extra                                             # ПОСТОРОННИЕ ЗАТРАТЫ
                  ],

              ]
              }

            input_data = json.dumps(data)

            headers = { 'Content-Type': 'application/json' }
        headers['Authorization'] = f'Bearer {key}'

        resp = requests.post(scoring_uri, input_data, headers=headers)
        fixed = float(re.findall(r"\d+\.\d+", resp.text)[0])

        if water != 0:
          fixed = (fixed * hectares / d.HECT_COEF[prov])*(-m.log(water / 100)) / 2
        else:
          fixed = (fixed * hectares / d.HECT_COEF[prov])
        if fixed <= 10:
          return 0
        return fixed*/
    /*
    def main(prov, hectares, town, water):
      forest = 100 - town - water

      var = variable(prov, hectares, town, forest, water)
      fixed = fixedC(prov, hectares, var, water)
      return fixed, var*/






    /*
    def __init__(self):
            self.PROV = {
                'Canada':0,
                'Ontario':1,
                'British Columbia' :2,
                'Quebec':3,
                'Alberta':4,
                'Manitoba':5,
                'Nova Scotia':6,
                'New Brunswick':7,
                'Saskatchewan':8,
                'Newfoundland and Labrador':9,
                'Yukon':10,
                'Northwest Territories':11,
                'Prince Edward Island':12,
                'Nunavut': 13}

            self.HECT_COEF = {
                0: 75.940546935,
                1: 25.47817458,
                2: 39.32252245,
                3: 50.07386497,
                4: 75.8843514,
                5: 21.40350151,
                6: 75.940546935,
                7: 75.940546935,
                8: 7.9709706229999995,
                9: 75.940546935,
                10: 10.18072289,
                11: 15.78476854,
                12: 75.940546935,
                13: 75.940546935
                }

            self.HARDWOOD = {
                0:0.07,
                1:0.36,
                2:0.04,
                3:0.38,
                4:0.44,
                5:0.37,
                6:0.28,
                7:0.36,
                8:0.43,
                9:0.03,
                10:0.08,
                11:0.10,
                12:0.35,
                13:0.10
                }

            self.SOFTWOOD = {
                0:0.03,
                1:0.64,
                2:0.96,
                3:0.62,
                4:0.56,
                5:0.63,
                6:0.72,
                7:0.64,
                8:0.57,
                9:0.97,
                10:0.92,
                11:0.9,
                12:0.65,
                13:0.9
                }

            self.variable = {
                4:{'Railroad':3.33277787035495,
                    'Road': 33.3344442592902,
                    'Town': 23.3327778703549},
                2:{'Railroad':1.80036007201436,
                    'Road': 18,
                    'Town': 16.1996399279856},
                5:{'Railroad':4.4166666666667,
                    'Road': 10.4166666666667,
                    'Town': 14.5833333333333},
                7:{'Railroad':5.33277787035495,
                    'Road': 29.4963129608801,
                    'Town': 29.5036870391199},
                11:{'Railroad':8.33277787035495,
                    'Road': 54,
                    'Town': 27},
                1:{'Railroad':6.54515703831643,
                    'Road': 32.7225126130632,
                    'Town': 32.7323303486204},
                3:{'Railroad':5.15789473684209,
                    'Road': 46.4210526315788,
                    'Town': 46.4210526315791},
                8:{'Railroad':2.33277787035495,
                    'Road': 24,
                    'Town': 24},
                6:{'Railroad':2.33277787035495,
                    'Road': 24,
                    'Town': 24},
                0:{'Railroad':2.33277787035495,
                    'Road': 24,
                    'Town': 24},
                9:{'Railroad':2.33277787035495,
                    'Road': 24,
                    'Town': 24},
                10:{'Railroad':4.33277787035495,
                    'Road': 44.5,
                    'Town': 44.5},
                12:{'Railroad':2.33277787035495,
                    'Road': 24,
                    'Town': 24},
                13:{'Railroad':2.33277787035495,
                    'Road': 24,
                    'Town': 24},
            }

    */




}
