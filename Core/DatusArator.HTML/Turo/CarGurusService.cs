using DatusArator.Core.Http;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DatusArator.HTML.Turo {
  public class CarGurusService {
    public static decimal? LookupValue(string make, string model, string year) {
      return Instance.LookupValueInt(make, model, year);
    }

    public static String LastError { get; private set; }

    public static CarGurusService Instance { get; } = new CarGurusService();

    private Dictionary<string, string> Makes;
    private Dictionary<string, Dictionary<string, string>> MakeModels;
    private Dictionary<string, Dictionary<string, string>> ModelYears;

    private CookieContainer Cookies;

    private CarGurusService() {
      Makes = new Dictionary<string, string>();
      MakeModels = new Dictionary<string, Dictionary<string, string>>();
      ModelYears = new Dictionary<string, Dictionary<string, string>>();

      InitMakesAndModels();
    }

    private void InitMakesAndModels() {
      Cookies = new CookieContainer();

      var options = new GetUriOptions();
      options.Method = HttpMethod.Get;
      options.Uri = new Uri(@"https://www.cargurus.com/Cars/instantMarketValue.action");
      options.CookieContainer = Cookies;

      var html = WebUtils.GetUri(options);
      var parser = new HtmlParser(null, html);

      var nodes = parser.SelectNodesRaw("select#carPicker_makerSelect option");
      foreach (var option in nodes) {
        Makes[option.InnerText.ToLower()] = option.Attributes["value"]?.Value;
      }

      options = new GetUriOptions();
      options.Method = HttpMethod.Get;
      options.Uri = new Uri(@"https://www.cargurus.com/Cars/getCarPickerReferenceDataAJAX.action?showInactive=false&newCarsOnly=false&useInventoryService=true&quotableCarsOnly=false&carsWithRegressionOnly=false&localeCountryCarsOnly=true");
      options.CookieContainer = Cookies;
      var json = WebUtils.GetUri(options);
      var modelsWrapper = new JsonWrapper(json);

      for (int i = 1; i < 10000; i++) {
        var key = "m" + i;
        var wrapper = new JsonWrapper(modelsWrapper, "allMakerModels." + key);

        var models = new Dictionary<string, string>();
        AddModels(models, wrapper.GetArrayAsObjects("popular"));
        AddModels(models, wrapper.GetArrayAsObjects("unpopular"));
        if (models.Count > 0)
          MakeModels[key] = models;
      }
    }

    private void AddModels(Dictionary<string, string> models, List<JsonWrapper> list) {
      if (list == null)
        return;

      foreach (var model in list) {
        models[model.Get("modelName").ToLower()] = model.Get("modelId");
      }
    }

    private decimal? LookupValueInt(string make, string model, string year) {
      LastError = null;

      var yearsData = LookupYear(make, model);
      if (yearsData == null)
        return null;

      if (!yearsData.ContainsKey(year)) {
        LastError = "Unable to Find Year: " + make + "[" + model + "] (" + year + ")";
        return null;
      }

      var carId = yearsData[year];

      var options = new GetUriOptions();
      options.Method = HttpMethod.Post;
      options.Uri = new Uri(@"https://www.cargurus.com/Cars/generateCarValuesReportStubAjax.action");
      options.CookieContainer = Cookies;

      options.Referer = @"https://www.cargurus.com/Cars/instantMarketValue.action";
      options.Headers.Add(@"X-Requested-With: XMLHttpRequest");
      options.Headers.Add(@"origin: https://www.cargurus.com");


      var postData = @"carDescription.autoEntityId=" + carId +
                     @"&carDescription.postalCode=&carDescription.mileage=&carDescription.price=" +
                     @"&carDescription.radius=75&carDescription.transmission=1&forPrivateListing=false" + 
                     @"&carDescription.vin=&selectedEntity=" + carId + "&carDescription.engineId=-1";
      options.PostData = Encoding.ASCII.GetBytes(postData);
      options.PostContentType = @"application/x-www-form-urlencoded; charset=UTF-8";

      var html = WebUtils.GetUri(options);
      var parser = new HtmlParser(null, html);

      var node = parser.SelectFirstNodeRaw("div#pcc-instantValue-basic h3 span");
      return StringUtils.SafeStrToDecimal(node?.InnerText, null, true);
    }

    private Dictionary<string, string> LookupYear(string make, string model) {
      if (!Makes.ContainsKey(make.ToLower())) { 
        LastError = "Invalid Make: " + make;
        return null;
      }
      var makeId = Makes[make.ToLower()];

      var makeModels = MakeModels[makeId];
      if (!makeModels.ContainsKey(model.ToLower())) {
        LastError = "Invalid Model: " + make + " [" + model + "]";
        return null;
      }
      var modelId = makeModels[model.ToLower()];

      if (ModelYears.ContainsKey(modelId))
        return ModelYears[modelId];

      var yearUrl = @"https://www.cargurus.com/Cars/getSelectedMakerModelCarsAJAX.action?maker={MAKER}&showInactive=false&newCarsOnly=false&useInventoryService=true&quotableCarsOnly=false&carsWithRegressionOnly=false&localeCountryCarsOnly=true";
      yearUrl = yearUrl.Replace("{MAKER}", makeId);

      var options = new GetUriOptions();
      options.Method = HttpMethod.Get;
      options.Uri = new Uri(yearUrl);
      options.RetryOnFail = true;
      options.CookieContainer = Cookies;
      var yearWrapper = new JsonWrapper(WebUtils.GetUri(options));

      foreach (var key in yearWrapper.GetArrays().Keys) {
        var years = new Dictionary<string, string>();
        foreach (var year in yearWrapper.GetArrayAsObjects(key)) {
          years[year.Get("carName").ToLower()] = year.Get("carId");
        }

        if (years.Count > 0)
          ModelYears[key] = years;
      }

      if (ModelYears.ContainsKey(modelId))
        return ModelYears[modelId];
      else {
        LastError = "Error getting years for known model: " + make + " [" + model + "] {" + modelId + "}";
        return null;
      }
    }
  }
}
