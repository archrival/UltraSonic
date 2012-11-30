using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Subsonic.Rest.Api.Enums;

namespace Subsonic.Rest.Api
{
    public partial class SubsonicApi
    {
        /// <summary>
        /// Return a Subsonic Response object for the given Subsonic method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>Response</returns>
        private async Task<Response> RequestAsync(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            Response result;
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            try
            {
                string restResponse = null;

                using (var response = await request.GetResponseAsync())
                {
                    if (response != null)
                    {
                        if (response.ContentType.Contains("text/xml"))
                        {
                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            using (Stream stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                {
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = await streamReader.ReadToEndAsync();
                                }
                            }
                        }
                        else
                        {
                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "HTTP response does not contain XML, content type is: {0}", response.ContentType));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(restResponse))
                {
                    result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                    if (ServerApiVersion == null)
                        ServerApiVersion = Version.Parse(result.Version);
                }
                else
                {
                    throw new SubsonicApiException("Empty HTTP response returned.");
                }
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            return result;
        }

        /// <summary>
        /// Return a Subsonic Response object for the given Subsonic method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>Response</returns>
        private Response Request(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            Response result;
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);

            try
            {
                string restResponse = null;

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect))
                    {
                        if (response.ContentType.Contains("text/xml"))
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = streamReader.ReadToEnd();
                            }
                        }
                        else
                        {
                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "HTTP response does not contain XML, content type is: {0}", response.ContentType));
                        }
                    }
                    else
                    {
                        if (response != null)
                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Invalid HTTP response status code: {0}", response.StatusCode));
                    }
                }

                if (!string.IsNullOrEmpty(restResponse))
                {
                    result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                    if (ServerApiVersion == null)
                        ServerApiVersion = Version.Parse(result.Version);
                }
                else
                {
                    throw new SubsonicApiException("Empty HTTP response returned.");
                }
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            return result;
        }

        /// <summary>
        /// Save response to disk.
        /// </summary>
        /// <param name="path">Directory to save the response to, the filename is provided by the server using the Content-Disposition header.</param>
        /// <param name="pathOverride">If specified, the value of path becomes the complete path including filename.</param>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>long</returns>
        private async Task<long> RequestAsync(string path, bool pathOverride, Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            long bytesTransferred = 0;
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);
            bool download = true;

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            try
            {
                using (HttpWebResponse response = (HttpWebResponse) (await request.GetResponseAsync()))
                {
                    if (response != null)
                    {
                        if (!response.ContentType.Contains("text/xml"))
                        {
                            // Read the file name from the Content-Disposition header if a path override value was not provided
                            if (!pathOverride)
                            {
                                if (response.Headers.AllKeys.Contains("Content-Disposition"))
                                {
                                    var contentDisposition = new ContentDisposition(response.Headers["Content-Disposition"]);
                                    if (!string.IsNullOrEmpty(contentDisposition.FileName))
                                        path = Path.Combine(path, contentDisposition.FileName);
                                    else
                                        throw new SubsonicApiException("FileName was not provided in the Content-Disposition header, you must use the path override flag.");
                                }
                                else
                                {
                                    throw new SubsonicApiException("Content-Disposition header was not provided, you must use the path override flag.");
                                }
                            }
                        }
                        else
                        {
                            string restResponse = null;
                            var result = new Response();

                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            using (Stream stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                {
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = await streamReader.ReadToEndAsync();
                                }
                            }

                            if (!string.IsNullOrEmpty(restResponse))
                                result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                            if (result.ItemElementName == ItemChoiceType.Error)
                                throw new SubsonicErrorException("Error occurred during request.", result.Item as Error);

                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unexpected response type: {0}", Enum.GetName(typeof(ItemChoiceType), result.ItemElementName)));
                        }

                        if (File.Exists(path))
                        {
                            var fileInfo = new FileInfo(path);

                            // If the file on disk matches the file on the server, do not attempt a download
                            if (response.ContentLength >= 0 && response.LastModified == fileInfo.LastWriteTime && response.ContentLength == fileInfo.Length)
                                download = false;
                        }

                        if (!System.IO.Directory.Exists(Path.GetDirectoryName(path)))
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));

                        if (download)
                        {
                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            using (Stream stream = response.GetResponseStream())
                            using (FileStream fileStream = File.Create(path))
                            {
                                if (stream != null)
                                {
                                    if (cancelToken.HasValue)
                                        cancelToken.Value.ThrowIfCancellationRequested();

                                    await stream.CopyToAsync(fileStream);
                                    bytesTransferred = fileStream.Length;
                                }
                            }

                            File.SetLastWriteTime(path, response.LastModified);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            return bytesTransferred;
        }

        /// <summary>
        /// Make an async web request without waiting for a response
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>long</returns>
        private async Task<long> RequestAsyncNoResponse(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            try
            {
                using (await request.GetResponseAsync()) { }
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            return 0;
        }

        /// <summary>
        /// Save response to disk.
        /// </summary>
        /// <param name="path">Directory to save the response to, the filename is provided by the server using the Content-Disposition header.</param>
        /// <param name="pathOverride">If specified, the value of path becomes the complete path including filename.</param>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>long</returns>
        private long Request(string path, bool pathOverride, Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            long bytesTransferred = 0;
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);
            bool download = true;

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect))
                    {
                        if (!response.ContentType.Contains("text/xml"))
                        {
                            // Read the file name from the Content-Disposition header if a path override value was not provided
                            if (!pathOverride)
                            {
                                if (response.Headers.AllKeys.Contains("Content-Disposition"))
                                {
                                    var contentDisposition = new ContentDisposition(response.Headers["Content-Disposition"]);
                                    if (!string.IsNullOrEmpty(contentDisposition.FileName))
                                        path = Path.Combine(path, contentDisposition.FileName);
                                    else
                                        throw new SubsonicApiException("FileName was not provided in the Content-Disposition header, you must use the path override flag.");
                                }
                                else
                                {
                                    throw new SubsonicApiException("Content-Disposition header was not provided, you must use the path override flag.");
                                }
                            }
                        }
                        else
                        {
                            string restResponse = null;
                            var result = new Response();

                            using (Stream stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = streamReader.ReadToEnd();
                            }

                            if (!string.IsNullOrEmpty(restResponse))
                                result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                            if (result.ItemElementName == ItemChoiceType.Error)
                            {
                                throw new SubsonicErrorException("Error occurred during request.", result.Item as Error);
                            }

                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unexpected response type: {0}", Enum.GetName(typeof (ItemChoiceType), result.ItemElementName)));
                        }

                        if (File.Exists(path))
                        {
                            var fileInfo = new FileInfo(path);

                            // If the file on disk matches the file on the server, do not attempt a download
                            if (response.ContentLength >= 0 && response.LastModified == fileInfo.LastWriteTime && response.ContentLength == fileInfo.Length)
                                download = false;
                        }

                        if (!System.IO.Directory.Exists(Path.GetDirectoryName(path)))
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));

                        if (download)
                        {
                            using (Stream stream = response.GetResponseStream())
                            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                if (stream != null) stream.CopyTo(fileStream);
                                bytesTransferred = fileStream.Length;
                            }

                            File.SetLastWriteTime(path, response.LastModified);
                        }
                    }
                    else
                    {
                        if (response != null)
                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Invalid HTTP response status code: {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            return bytesTransferred;
        }

        /// <summary>
        /// Return an Image for the specified method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>Image</returns>
        private async Task<long> ImageSizeRequestAsync(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri, "GET");
            long length = -1;

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    if (response != null)
                    {
                        if (!response.ContentType.Contains("text/xml"))
                        {
                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            length = response.ContentLength;
                        }
                        else
                        {
                            string restResponse = null;
                            var result = new Response();

                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            using (Stream stream = response.GetResponseStream())
                                if (stream != null)
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = streamReader.ReadToEnd();

                            if (!string.IsNullOrEmpty(restResponse))
                                result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                            if (result.ItemElementName == ItemChoiceType.Error)
                                throw new SubsonicErrorException("Error occurred during request.", result.Item as Error);

                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unexpected response type: {0}", Enum.GetName(typeof(ItemChoiceType), result.ItemElementName)));
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                    return length;
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            return length;
        }

        /// <summary>
        /// Return an Image for the specified method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <param name="cancelToken"> </param>
        /// <returns>Image</returns>
        private async Task<Image> ImageRequestAsync(Methods method, Version methodApiVersion, ICollection parameters = null, CancellationToken? cancelToken = null)
        {
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);
            var content = new MemoryStream();

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    if (response != null)
                    {
                        if (!response.ContentType.Contains("text/xml"))
                        {
                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            using (Stream stream = response.GetResponseStream())
                                if (stream != null)
                                    await stream.CopyToAsync(content);
                        }
                        else
                        {
                            string restResponse = null;
                            var result = new Response();

                            if (cancelToken.HasValue)
                                cancelToken.Value.ThrowIfCancellationRequested();

                            using (Stream stream = response.GetResponseStream())
                                if (stream != null)
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = streamReader.ReadToEnd();

                            if (!string.IsNullOrEmpty(restResponse))
                                result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                            if (result.ItemElementName == ItemChoiceType.Error)
                                throw new SubsonicErrorException("Error occurred during request.", result.Item as Error);

                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unexpected response type: {0}", Enum.GetName(typeof(ItemChoiceType), result.ItemElementName)));
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                    return null;
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            if (cancelToken.HasValue)
                cancelToken.Value.ThrowIfCancellationRequested();

            return Image.FromStream(content);
        }

        /// <summary>
        /// Return an Image for the specified method.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>Image</returns>
        private Image ImageRequest(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            string requestUri = BuildRequestUri(method, methodApiVersion, parameters);

            HttpWebRequest request = BuildRequest(requestUri);
            Image image = default(Image);

            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect))
                    {
                        if (!response.ContentType.Contains("text/xml"))
                        {
                            using (Stream stream = response.GetResponseStream())
                                if (stream != null) image = Image.FromStream(stream);
                        }
                        else
                        {
                            string restResponse = null;
                            var result = new Response();

                            using (Stream stream = response.GetResponseStream())
                                if (stream != null)
                                    using (var streamReader = new StreamReader(stream))
                                        restResponse = streamReader.ReadToEnd();

                            if (!string.IsNullOrEmpty(restResponse))
                                result = XmlUtilities.DeserializeFromXml<Response>(restResponse);

                            if (result.ItemElementName == ItemChoiceType.Error)
                                throw new SubsonicErrorException("Error occurred during request.", result.Item as Error);

                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Unexpected response type: {0}", Enum.GetName(typeof (ItemChoiceType), result.ItemElementName)));
                        }
                    }
                    else
                    {
                        if (response != null)
                            throw new SubsonicApiException(string.Format(CultureInfo.CurrentCulture, "Invalid HTTP response status code: {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SubsonicApiException(ex.Message, ex);
            }

            return image;
        }

        /// <summary>
        /// Builds a URI to be used for the request.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>string</returns>
        private string BuildRequestUri(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            string request = string.Format(CultureInfo.InvariantCulture, "{0}/rest/{1}.view?v={2}&c={3}", ServerUrl, Enum.GetName(typeof (Methods), method), methodApiVersion, UserAgent);

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    string key = string.Empty;
                    string value = string.Empty;

                    if (parameter is DictionaryEntry)
                    {
                        DictionaryEntry entry = (DictionaryEntry) parameter;
                        key = entry.Key.ToString();
                        value = entry.Value.ToString();
                    }

                    if (parameter is KeyValuePair<string, string>)
                    {
                        KeyValuePair<string, string> entry = (KeyValuePair<string, string>)parameter;
                        key = entry.Key;
                        value = entry.Value;
                    }

                    request += string.Format(CultureInfo.InvariantCulture, "&{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value));
                }
            }

            return request;
        }

        /// <summary>
        /// Builds a URI to be used for the request.
        /// </summary>
        /// <param name="method">Subsonic API method to call.</param>
        /// <param name="methodApiVersion">Subsonic API version of the method.</param>
        /// <param name="parameters">Parameters used by the method.</param>
        /// <returns>string</returns>
        private string BuildRequestUriUser(Methods method, Version methodApiVersion, ICollection parameters = null)
        {
            string password = string.Format(CultureInfo.InvariantCulture, "enc:{0}", Strings.AsciiToHex(Password));

            string request = string.Format(CultureInfo.InvariantCulture, "{0}/rest/{1}.view?v={2}&c={3}&u={4}&p={5}", ServerUrl, Enum.GetName(typeof(Methods), method), methodApiVersion, UserAgent, UserName, password);

            if (parameters != null && parameters.Count > 0)
                request = parameters.Cast<DictionaryEntry>().Aggregate(request, (current, parameter) => current + string.Format(CultureInfo.InvariantCulture, "&{0}={1}", HttpUtility.UrlEncode(parameter.Key.ToString()), HttpUtility.UrlEncode(parameter.Value.ToString())));

            return request;
        }

        /// <summary>
        /// Build an HTTP request using the values provided in the class.
        /// </summary>
        /// <param name="requestUri">URI for the request.</param>
        /// <returns>HttpWebRequest</returns>
        private HttpWebRequest BuildRequest(string requestUri, string method = "POST")
        {
            var request = WebRequest.Create(requestUri) as HttpWebRequest;
            if (request != null)
            {
                request.UserAgent = UserAgent;
                request.Method = method;

                // Add credentials
                request.Credentials = new NetworkCredential(UserName, Password);

                // Add Authorization header
                string authInfo = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", UserName, Password);
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                request.Headers["Authorization"] = string.Format(CultureInfo.InvariantCulture, "Basic {0}", authInfo);

                // Add proxy information if specified, limit to valid ports
                if (!string.IsNullOrEmpty(ProxyServer) && (ProxyPort > 0 && ProxyPort < 65536))
                {
                    var proxy = new WebProxy(ProxyServer, ProxyPort);

                    if (!string.IsNullOrEmpty(ProxyUserName))
                    {
                        if (string.IsNullOrEmpty(ProxyPassword))
                            throw new SubsonicApiException("When specifying a proxy username, you must also specify a password.");

                        proxy.Credentials = new NetworkCredential(ProxyUserName, ProxyPassword);
                    }

                    request.Proxy = proxy;
                }
            }

            return request;
        }
    }
}