using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web.Http;

namespace IisHelperService.Controllers
{
    public class ExtractRequest
    {
        public string ZipPath { get; set; }
        public string ExtractPath { get; set; }
        public bool DeleteAfterExtract { get; set; }
    }

    [RoutePrefix("api/Zip")]
    public class ZipController : ApiController
    {
        private Result Execute(Action<Action<string>> action)
        {
            var stb = new StringBuilder();
            Action<string> act = (x) => stb.AppendLine(x);
            try
            {
                action(act);
                return new Result { Success = true, Message = stb.ToString() };
            }
            catch (Exception ex)
            {
                act(ex.Message);
                act(ex.StackTrace);
                return new Result { Success = false, Message = stb.ToString() };
            }
        }

        [HttpPost]
        [Route("Extract")]
        public Result Extract([FromBody] ExtractRequest request)
        {
            return Execute((act) =>
            {
                ZipFile.ExtractToDirectory(request.ZipPath, request.ExtractPath);
                if (request.DeleteAfterExtract)
                {
                    File.Delete(request.ZipPath);
                }
                act("SUCCESS!");
            });
        }
    }
}
