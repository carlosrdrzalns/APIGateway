using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using APIGateway.Models;
using System.Text.Json;
using System.Text;
using System.Net;
using System.Globalization;

namespace APIGateway.Controllers
{

    public class APIGatewayController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();

        public APIGatewayController()
        {
        }

        #region DataIngest

        [HttpPost]
        [Route("BiologicalReactor/postData")]
        public async Task<ActionResult> postBiologicalData([FromBody] BiologicalReactorData data)
        {
            try
            {
                using StringContent jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://localhost:44387/BiologicalReactorController/postData", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }
            }catch(System.Exception exp)
            {
                return NotFound(exp);
            }
            
        }

        [HttpGet]
        [Route("BiologicalReactor/getData")]
        public async Task<ActionResult<List<BiologicalReactorData>>> getBiologicalData(int nElements)
        {
            try
            {              
                var parameters = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "nElements", nElements }
                };

                string queryString = string.Join("&", parameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:44387/BiologicalReactorController/getData?"+queryString);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    List<BiologicalReactorData> data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BiologicalReactorData>>(jsonData);
                    return Ok(data);
                    
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }
            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        [HttpPost]
        [Route("WaterPump/postData")]
        public async Task<ActionResult> postWaterPumpData([FromBody] WaterPumpData data)
        {
            try
            {
                using StringContent jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://localhost:44387/waterpumpcontroller/postData", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }
                //Despues de publicar la información, debemos pedir una predicción al motor de IA
                List<WaterPumpData> statusDatas = await getWaterPumpData(10);
                WaterPumpStatusPredictions waterPumpStatusPredictions = await PostRequestPrediction(statusDatas);
                ActionResult actionResult = await postPredictionData(waterPumpStatusPredictions);
                return actionResult;


            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        public async Task<WaterPumpStatusPredictions> PostRequestPrediction(List<WaterPumpData> statusData)
        {
            try
            {
                using StringContent jsonContentPrediction = new StringContent(JsonSerializer.Serialize(statusData), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/MLAlgorithm", jsonContentPrediction);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                //Los resultados de la predicción tenemos que publicarlos en la base de datos
                string jsonData = await response.Content.ReadAsStringAsync();
                WaterPumpStatusPredictions predictionData = new WaterPumpStatusPredictions()
                {
                    Id = Guid.NewGuid(),
                    Status = Convert.ToDouble(jsonData, CultureInfo.InvariantCulture),
                    timestamp = statusData.Select(wpd => wpd.timestamp).LastOrDefault().AddSeconds(30 * 10)
                };
                return predictionData;
            } catch(System.Exception exp)
            {
                return null;
            }
                   
        }

        public async Task<ActionResult> postPredictionData(WaterPumpStatusPredictions waterPumpStatusPredictions)
        {
            using StringContent jsonContentPredictionResult = new StringContent(JsonSerializer.Serialize(waterPumpStatusPredictions), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("https://localhost:44387/waterpumpcontroller/postPredictions", jsonContentPredictionResult);
            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return NotFound();
                }
            }
            return Ok();
        }

        [HttpGet]
        [Route("WaterPump/getData")]
        public async Task<ActionResult<List<WaterPumpData>>> getAsyncWaterPumpData(int nElements)
        {
            try
            {
                List<WaterPumpData> data = await getWaterPumpData(nElements);
                return Ok(data);
            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }
        }

        public async Task<List<WaterPumpData>> getWaterPumpData(int nElements)
        {
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "nElements", nElements }
                };

                string queryString = string.Join("&", parameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:44387/waterpumpcontroller/getData?" + queryString);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    List<WaterPumpData> data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WaterPumpData>>(jsonData);
                    return data;

                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception exp)
            {
                return null;
            }

        }

        [HttpGet]
        [Route("WaterPumpPredictions/getData")]
        public async Task<ActionResult<List<WaterPumpStatusPredictions>>> getWaterPumpPredictionsData(int nElements)
        {
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "nElements", nElements }
                };

                string queryString = string.Join("&", parameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:44387/waterpumpcontroller/getPredictions?" + queryString);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    List<WaterPumpStatusPredictions> data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WaterPumpStatusPredictions>>(jsonData);
                    return Ok(data);

                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }
            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        #endregion

        #region Authentication


        [HttpGet]
        [Route("Authentication/getToken")]
        public async Task<ActionResult<Guid>> getToken(string username, string password)
        {
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "username", username },
                    {"password", password }
                };

                string queryString = string.Join("&", parameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:44346/authenticationUser?" + queryString);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    Guid data = Newtonsoft.Json.JsonConvert.DeserializeObject<Guid>(jsonData);
                    return Ok(data);

                }
                else
                {

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }

                }
            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        [HttpGet]
        [Route("Authentication/checkToken")]
        public async Task<ActionResult> checkToken(Guid token)
        {
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, Guid>
                {
                    { "token", token }
                };

                string queryString = string.Join("&", parameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:44346/authenticationToken?" + queryString);
                if (response.IsSuccessStatusCode)
                {

                    return Ok();

                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }
            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        [HttpGet]
        [Route("Authentication/checkDevice")]
        public async Task<ActionResult> checkDevice(string deviceName, string deviceKey)
        {
            try
            {
                var parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "deviceName", deviceName },
                    {"deviceKey", deviceKey }
                };

                string queryString = string.Join("&", parameters
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}"));

                HttpResponseMessage response = await client.GetAsync("https://localhost:44346/authenticationDevice?" + queryString);
                if (response.IsSuccessStatusCode)
                {
                   
                    return Ok();

                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }
            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        #endregion

        #region AutodeskPlatformServices

        [HttpGet]
        [Route("APS/getToken")]
        public async Task<dynamic> getAPSToken()
        {
            try
            {

                HttpResponseMessage response = await client.GetAsync("https://localhost:44308/api/forge/oauth/2leggedtoken");
                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized();
                        case HttpStatusCode.BadRequest:
                            return BadRequest();
                        default:
                            return NotFound();
                    }
                }

                string jsonData = await response.Content.ReadAsStringAsync();
                Autodesk.Forge.Model.DynamicJsonResponse data = Newtonsoft.Json.JsonConvert.DeserializeObject<Autodesk.Forge.Model.DynamicJsonResponse>(jsonData);
                return data.Dictionary;


            }
            catch (System.Exception exp)
            {
                return NotFound(exp);
            }

        }

        #endregion

    }

}
