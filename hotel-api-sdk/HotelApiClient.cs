﻿using System;
using System.Text;

using com.hotelbeds.distribution.hotel_api_model.auto.messages;
using com.hotelbeds.distribution.hotel_api_sdk.types;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace com.hotelbeds.distribution.hotel_api_sdk
{
    /// <summary>
    /// 
    /// </summary>
    public class HotelApiClient
    {
        //https://developer.hotelbeds.com/docs/read/apitude_booking/

        /// <summary>
        /// Atributos
        /// </summary>
        private readonly TimeSpan timeout;
        private readonly string basePath;
        private readonly HotelApiVersion version;
        private readonly HotelApiVersion paymentVersion;
        private readonly string apiKey;
        private readonly string sharedSecret;
        private string environment;

        public HotelApiClient()
        {
            this.timeout = GetTimeoutFromConfig();
            this.apiKey = GetHotelApiKeyFromConfig();
            this.sharedSecret = GetHotelSharedSecretFromConfig();
            this.version = new HotelApiVersion(HotelApiVersion.versions.V1);
            this.paymentVersion = new HotelApiVersion(HotelApiVersion.versions.V1_1);
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(HotelApiVersion version)
        {
            this.timeout = GetTimeoutFromConfig();
            this.apiKey = GetHotelApiKeyFromConfig();
            this.sharedSecret = GetHotelSharedSecretFromConfig();
            this.version = version;
            this.paymentVersion = new HotelApiVersion(HotelApiVersion.versions.V1_1);
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(HotelApiVersion version, HotelApiVersion paymentVersion)
        {
            this.timeout = GetTimeoutFromConfig();
            this.apiKey = GetHotelApiKeyFromConfig();
            this.sharedSecret = GetHotelSharedSecretFromConfig();
            this.version = version;
            this.paymentVersion = paymentVersion;
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(string apiKey, string sharedSecret)
        {
            this.timeout = GetTimeoutFromConfig();
            this.apiKey = apiKey;
            this.sharedSecret = sharedSecret;
            this.version = new HotelApiVersion(HotelApiVersion.versions.V1);
            this.paymentVersion = new HotelApiVersion(HotelApiVersion.versions.V1_1);
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(HotelApiVersion version, string apiKey, string sharedSecret)
        {
            this.timeout = GetTimeoutFromConfig();
            this.apiKey = apiKey;
            this.sharedSecret = sharedSecret;
            this.version = version;
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        public HotelApiClient(HotelApiVersion version, HotelApiVersion paymentVersion, string apiKey, string sharedSecret)
        {
            this.timeout = GetTimeoutFromConfig();
            this.apiKey = apiKey;
            this.sharedSecret = sharedSecret;
            this.version = version;
            this.paymentVersion = paymentVersion;
            this.basePath = GetEnvironment();
            CheckHotelApiClientConfig();
        }

        private void CheckHotelApiClientConfig()
        {
            if (String.IsNullOrEmpty(this.apiKey) || version == null || String.IsNullOrEmpty(this.basePath) || String.IsNullOrEmpty(this.sharedSecret))
                throw new Exception("HotelApiClient cannot be created without specifying an API key, Shared Secret, the Hotel API version and the service you are connecting to.", new ArgumentNullException());
        }

        private string GetHotelApiKeyFromConfig()
        {
            try
            {
                string returnValue = ConfigurationManager.AppSettings.Get("ApiKey");

                return returnValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private TimeSpan GetTimeoutFromConfig()
        {
            try
            {
                int returnValue = int.Parse(ConfigurationManager.AppSettings.Get("TimeoutSeconds"));
                return TimeSpan.FromSeconds(returnValue);
            }
            catch
            {
                // In case the client updated the version and did not configure the new parameter
                return TimeSpan.FromSeconds(5000);
            }
        }

        private string GetHotelSharedSecretFromConfig()
        {
            try
            {
                string returnValue = ConfigurationManager.AppSettings.Get("SharedSecret");

                return returnValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetEnvironment()
        {
            try
            {
                string returnValue = string.Empty;
                environment = ConfigurationManager.AppSettings.Get("ENVIRONMENT");
                if (!String.IsNullOrEmpty(environment))
                {
                    returnValue = ConfigurationManager.AppSettings.Get(environment);
                }
                return returnValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public StatusRS status()
        {
            HotelApiPaths.STATUS avail = new HotelApiPaths.STATUS();
            StatusRS status = callRemoteApi<StatusRS, Object>(null, avail, null, this.version);
            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AvailabilityRS doAvailability(AvailabilityRQ request)
        {
            try
            {
                HotelApiPaths.AVAILABILITY avail = new HotelApiPaths.AVAILABILITY();
                AvailabilityRS response = callRemoteApi<AvailabilityRS, AvailabilityRQ>(request, avail, null, this.version);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public CheckRateRS doCheck(CheckRateRQ checkRateRQ)
        {
            try
            {
                HotelApiPaths.CHECK_AVAIL checkRate = new HotelApiPaths.CHECK_AVAIL();
                CheckRateRS response = callRemoteApi<CheckRateRS, CheckRateRQ>(checkRateRQ, checkRate, null, this.version);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingRS confirm(BookingRQ bookingRQ)
        {
            try
            {
                HotelApiPaths.BOOKING_CONFIRM bookingConfirm = new HotelApiPaths.BOOKING_CONFIRM();
                HotelApiVersion version = this.version;
                if(bookingRQ!=null&& bookingRQ.paymentData!=null)
                {
                    version = paymentVersion;
                }
                BookingRS response = callRemoteApi<BookingRS, BookingRQ>(bookingRQ, bookingConfirm, null, version);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingCancellationRS Cancel(List<Tuple<string, string>> param)
        {
            try
            {
                HotelApiPaths.BOOKING_CANCEL bookingCancel = new HotelApiPaths.BOOKING_CANCEL();
                BookingCancellationRS response = callRemoteApi<BookingCancellationRS, Tuple<string, string>[]>(null, bookingCancel, param, this.version);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingDetailRS Detail(List<Tuple<string, string>> param)
        {
            try
            {
                HotelApiPaths.BOOKING_DETAIL bookingDetail = new HotelApiPaths.BOOKING_DETAIL();
                BookingDetailRS response = callRemoteApi<BookingDetailRS, Tuple<string, string>[]>(null, bookingDetail, param, this.version);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        public BookingListRS List(List<Tuple<string, string>> param)
        {
            try
            {
                HotelApiPaths.BOOKING_LIST bookingList = new HotelApiPaths.BOOKING_LIST();
                BookingListRS response = callRemoteApi<BookingListRS, Tuple<string, string>[]>(null, bookingList, param, this.version);
                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }

        private T callRemoteApi<T, U>(U request, HotelApiPaths.HotelApiPathsBase path, List<Tuple<string, string>> param, HotelApiVersion version)
        {
            try
            {
                T response = default(T);

                using (var client = new HttpClient(
                                        new HttpClientHandler()
                                        {
                                            AutomaticDecompression = System.Net.DecompressionMethods.GZip
                                        }))
                {
                    if (request == null && (path.GetType() != typeof(HotelApiPaths.STATUS)
                        && path.GetType() != typeof(HotelApiPaths.BOOKING_CANCEL) && path.GetType() != typeof(HotelApiPaths.BOOKING_DETAIL) && path.GetType() != typeof(HotelApiPaths.BOOKING_LIST)))
                        throw new Exception("Object request can't be null");

                    client.BaseAddress = new Uri(path.getUrl(this.basePath, version));
                    client.DefaultRequestHeaders.Clear();
                    client.Timeout = this.timeout;
                    client.DefaultRequestHeaders.Add("Api-Key", this.apiKey);

                    long ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000;
                    SHA256 hashstring = SHA256Managed.Create();
                    byte[] hash = hashstring.ComputeHash(Encoding.UTF8.GetBytes(this.apiKey + this.sharedSecret + ts));
                    string signature = BitConverter.ToString(hash).Replace("-", "");
                    client.DefaultRequestHeaders.Add("X-Signature", signature.ToString());
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json; charset=utf-8");

                    // GET Method
                    if (path.getHttpMethod() == HttpMethod.Get)
                    {
                        string Uri = path.getEndPoint();

                        if (param != null)
                        {
                            Uri = path.getEndPoint(param);
                        }

                        HttpResponseMessage resp = client.GetAsync(Uri).Result;
                        response = resp.Content.ReadAsAsync<T>().Result;
                        return response;
                    }

                    // DELETE Method
                    if (path.getHttpMethod() == HttpMethod.Delete)
                    {
                        string Uri = path.getEndPoint();

                        if (param != null)
                        {
                            Uri = path.getEndPoint(param);
                        }

                        HttpResponseMessage resp = client.DeleteAsync(Uri).Result;
                        response = resp.Content.ReadAsAsync<T>().Result;
                        return response;
                    }

                    StringContent contentToSend = null;
                    if (request != null)
                    {
                        string objectSerialized = JsonConvert.SerializeObject(request, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });
                        contentToSend = new StringContent(objectSerialized, Encoding.UTF8, "application/json");
                    }

                    if (path.getHttpMethod() == HttpMethod.Post)
                    {
                        HttpResponseMessage resp = null;
                        if (param == null)
                            resp = client.PostAsync(path.getEndPoint(), contentToSend).Result;
                        else
                            resp = client.PostAsync(path.getEndPoint(param), contentToSend).Result;

                        response = resp.Content.ReadAsAsync<T>().Result;
                    }
                }

                return response;
            }
            catch (HotelSDKException e)
            {
                throw e;
            }
        }
    }
}
