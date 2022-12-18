﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel.Context.Models;
using Travel.Shared.ViewModels;
using Travel.Shared.ViewModels.Travel;

namespace Travel.Data.Interfaces
{
    public interface INews
    {
        Response UploadBanner(string name,IFormCollection frmdata, ICollection<IFormFile> files);
        Response GetBanner();
        Response GetBannerAll();
        Response DeleteBanner(Guid idBanner);
        Task<Response> GetApiWeather(string location);
        Task<Response> TranslateLang(string input, string fromLang, string toLang);
        Task<Response> GetGoogleMapLocationRes(string address);
        Task<Datum> GetGoogleMapLocation(string address);

        Response SearchBanner(JObject frmData);

    }
}
