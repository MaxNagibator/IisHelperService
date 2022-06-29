using System;
using System.Collections.Generic;
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
        public DuplicateAction DuplicateAction { get; set; }
    }

    public enum DuplicateAction
    {
        None = 0,
        Overwrite = 1,
        SkipIfExists = 2,
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
                var temp2 = DateTime.Now.ToString("yyyyMMdd") + Guid.NewGuid().ToString("N");
                var dir = Path.GetDirectoryName(request.ZipPath);
                var temp = Path.Combine(dir, temp2);
                act("extract from " + request.ZipPath + " to " + temp);
                ZipFile.ExtractToDirectory(request.ZipPath, temp);
                if (request.DeleteAfterExtract)
                {
                    act("delete " + request.ZipPath);
                    File.Delete(request.ZipPath);
                }
                act("MoveDirectory " + temp + " to " + request.ExtractPath + " duplicateAction:" + request.DuplicateAction.ToString());
                if (request.DuplicateAction == DuplicateAction.None)
                {
                    Directory.Move(temp, request.ExtractPath);
                }
                else
                {
                    MoveDirectory(temp, request.ExtractPath, request.DuplicateAction);
                }
                act("SUCCESS!");
            });
        }

        private void MoveDirectory(string source, string target, DuplicateAction duplicateAction)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));


                    if (duplicateAction != DuplicateAction.SkipIfExists || !File.Exists(targetFile))
                    {
                        var overwrite = duplicateAction == DuplicateAction.Overwrite;
                        if (overwrite && File.Exists(targetFile))
                        {
                            File.Delete(targetFile);
                        }

                        File.Move(file, targetFile);
                    }
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }

            Directory.Delete(source, true);
        }

        public class Folders
        {
            public string Source { get; private set; }
            public string Target { get; private set; }

            public Folders(string source, string target)
            {
                Source = source;
                Target = target;
            }
        }
    }
}
