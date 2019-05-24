using SIS.HTTP.Common;
using SIS.HTTP.Enums;
using SIS.HTTP.Exceptions;
using SIS.HTTP.Headers;
using SIS.HTTP.Headers.Contracts;
using SIS.HTTP.Requests.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SIS.HTTP.Requests
{
    public class HttpRequest : IHttpRequest
    {
        public HttpRequest(string requestString)
        {
            CoreValidator.ThrowIfNullOrEmpty(requestString, nameof(requestString));

            this.FormData = new Dictionary<string, object>();
            this.QueryData = new Dictionary<string, object>();
            this.Headers = new HttpHeaderCollection();

            this.ParseRequest(requestString);
        }

        public string Path { get; private set; }

        public string Url { get; private set; }

        public Dictionary<string, object> FormData { get; }

        public Dictionary<string, object> QueryData { get; }

        public IHttpHeaderCollection Headers { get; }

        public HttpRequestMethod RequestMethod { get; private set; }

        private bool IsValidRequestLine(string[] requestLineParams)
        {
           if(requestLineParams.Length != 3 || requestLineParams[2] != GlobalConstants.HttpOneProtocolFragment)
            {
                return false;
            }

            return true;
        }

        private bool IsValidRequestQueryString(string queryString)
        {
            string pattern = @"([a-z]*=?\d*[a-z]*&?)";

            Regex validateQueryString = new Regex(pattern);

            bool isValid = validateQueryString.IsMatch(queryString);

            string[] queryObjects = queryString.Split(new[] { '&' }, StringSplitOptions.None);

            foreach (var obj in queryObjects)
            {
                string[] objParams = obj.Split(new[] { '=' }, StringSplitOptions.None);

                if(objParams.Length != 2)
                {
                    isValid = false;
                }

            }

            return isValid;
        }

        private void ParseRequestMethod(string[] requestLineParams)
        {
            string methodName = requestLineParams[0];
            HttpRequestMethod requestMethod;

            bool isParsed = Enum.TryParse(methodName, true, out requestMethod);

            if (!isParsed)
            {
                throw new BadRequestException();
            }

            this.RequestMethod = requestMethod;
        }

        private void ParseRequestUrl(string[] requestLineParams)
        {
            if (string.IsNullOrEmpty(requestLineParams[1]))
            {
                throw new BadRequestException();
            }

            this.Url = requestLineParams[1];
        }

        private void ParseRequestPath()
        {
            string[] urlSplit = this.Url.Split(new[] { '?' }, StringSplitOptions.None);

            if (string.IsNullOrEmpty(urlSplit[0]))
            {
                throw new BadRequestException();
            }

            this.Path = urlSplit[0];

        }

        private void ParseHeaders(string[] headers)
        {
            if (!headers.Any())
            {
                throw new BadRequestException();
            }

            foreach (var requestHeader in headers)
            {
                if (string.IsNullOrEmpty(requestHeader))
                {
                    return;
                }

                

                string[] splitHeader = requestHeader
                    .Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                string key = splitHeader[0];
                string value = splitHeader[1];

                HttpHeader header = new HttpHeader(key, value);

                this.Headers.AddHeader(header);
            }

            if (!this.Headers.ContainsHeader(GlobalConstants.HostHeaderKey))
            {
                throw new BadRequestException();
            }
        }

        private void ParseCookies()
        {

        }

        private void ParseQueryParameters()
        {
            //string queryString = this.Url
            //    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];

            string[] querySplit = this.Url
                .Split(new[] { '?', '#' }, StringSplitOptions.None);

            if(querySplit.Length == 1)
            {
                return;
            }

            if (!IsValidRequestQueryString(querySplit[1]))
            {
                throw new BadRequestException();
            }

            string[] queryObjects = querySplit[1]
                .Split(new[] { '&' }, StringSplitOptions.None);

            foreach (var obj in queryObjects)
            {
                string[] objParams = obj.Split(new[] { '=' }, StringSplitOptions.None);

                string key = objParams[0];
                string value = objParams[1];

                QueryData.Add(key, value);
            }


        }

        private void ParseFormDataParameters(string formData)
        {
            if (string.IsNullOrEmpty(formData))
            {
                return;
            }

            string[] requestBodyObjects = formData.Split(new[] { '&' }, StringSplitOptions.None);

            requestBodyObjects.Select(obj =>
            obj.Split(new[] { '&' }, StringSplitOptions.None))
            .ToList()
            .ForEach(bodyParameter =>
            this.FormData.Add(bodyParameter[0], bodyParameter[1]));


        }

        private void ParseRequestParameters(string formData)
        {
            this.ParseQueryParameters();
            this.ParseFormDataParameters(formData);
        }

        private void ParseRequest(string requestString)
        {
            string[] requestContent = requestString.
                Split(new[] { GlobalConstants.HttpNewLine }, StringSplitOptions.None);

            string[] requestLineParams = requestContent[0].Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!this.IsValidRequestLine(requestLineParams))
            {
                throw new BadRequestException();
            }

            this.ParseRequestMethod(requestLineParams);
            this.ParseRequestUrl(requestLineParams);
            this.ParseRequestPath();

            this.ParseHeaders(requestContent.Skip(1).ToArray());
            this.ParseCookies();

            this.ParseRequestParameters(requestContent[requestContent.Length - 1]);
        }
    }
}
