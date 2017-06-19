using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GitHubRepoList.Controllers
{
    public class RepoController : Controller
    {
        private RepoDBContext db = new RepoDBContext();

        // GET: Repo
        [ReadRightsRequired]
        public ActionResult Index()
        {
            var repos = db.Repos.Include("owner").OrderBy(r => r.sort_position);
            var json = new JavaScriptSerializer().Serialize(repos);
            return Content(json);
        }

        // GET: Repo/Details/5
        [ReadRightsRequired]
        public ActionResult Details(int id)
        {
            var repo = db.Repos.Include("owner").Where(r => r.id == id).FirstOrDefault();
            var json = new JavaScriptSerializer().Serialize(repo);
            return Content(json);
        }

        // POST: Repo/Create
        [HttpPost]
        public ActionResult Create(Repo newRepo)
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    var binUserpicIS = Request.Files.Get(0).InputStream;
                    newRepo.owner.binary_userpic = new byte[binUserpicIS.Length];
                    binUserpicIS.Read(newRepo.owner.binary_userpic, 0, (int) binUserpicIS.Length);      // Data loss inc
                }
                db.Repos.Add(newRepo);
                db.SaveChanges();
                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail", message = ex.Message });
            }
        }

        // POST: Repo/Edit/5
        [HttpPost]
        [WriteRightsRequired]
        public ActionResult Edit(int id, Repo repo)
        {
            try
            {
                db.Repos.Attach(repo);
                db.Entry(repo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail", message = ex.Message });
            }
        }

        // POST: Repo/EditSortPositions
        [HttpPost]
        [WriteRightsRequired]
        public ActionResult EditSortPositions(Repo[] repos)
        {
            try
            {
                foreach (var repo in repos)
                {
                    db.Repos.Attach(repo);
                    var entry = db.Entry(repo);
                    entry.Property(r => r.sort_position).IsModified = true;
                    db.SaveChanges();
                }

                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail", message = ex.Message });
             }
        }

        // POST: Repo/Delete
        [HttpPost]
        [WriteRightsRequired]
        public ActionResult Delete(int[] ids)
        {
            try
            {
                var reposToRemove = db.Repos.Include("owner").Where(r => ids.Contains(r.id));

                if (reposToRemove != null)
                {
                    foreach (var repo in reposToRemove)
                    {
                        db.RepoOwners.Remove(repo.owner);
                    }

                    db.Repos.RemoveRange(reposToRemove);
                    db.SaveChanges();

                    return Json(new { status = "OK" });
                }

                Logger.WriteLog("Can't find selected repos: " + string.Join(",", ids), LoggerLevel.INFO);
                return Json(new { status = "Fail", message = "Can't find selected repos" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail", message = ex.Message });
            }
        }

        // POST: Repo/ImportReposFromGithub

        [HttpPost]
        [WriteRightsRequired]
        public ActionResult ImportReposFromGithub(string login)
        {
            try
            {
                string html = string.Empty;
                string url = "https://api.github.com/users/" + login + "/repos";

                Repo[] repos = null;

                var client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var json = client.DownloadString(url);

                repos = new JavaScriptSerializer().Deserialize<Repo[]>(json);

                db.Repos.AddRange(repos);
                db.SaveChanges();

                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail" });
            }
        }

        // POST: Repo/ImportReposFromJson

        [HttpPost]
        [WriteRightsRequired]
        public ActionResult ImportReposFromJson(HttpPostedFileBase jsonFile)
        {
            try
            {
                Repo[] repos = null;
                string json = string.Empty;

                if (jsonFile.ContentLength > 0)
                {
                    Stream stream = jsonFile.InputStream;
                    using (var streamReader = new StreamReader(stream))
                    {
                        json = streamReader.ReadToEnd();
                    }
                }

                repos = new JavaScriptSerializer().Deserialize<Repo[]>(json);

                db.Repos.AddRange(repos);
                db.SaveChanges();

                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail", message = ex.Message });
            }
        }

        // GET: Repo/OwnerBinUserpic/Id

        public ActionResult OwnerBinUserpic(int id)
        {
            try
            {
                var ownerUserpic = db.Repos.Include("owner").Where(r => r.id == id).FirstOrDefault().owner.binary_userpic;

                if (ownerUserpic != null)
                {
                    return File(ownerUserpic, "image/png");
                }
                else
                {
                    return Redirect("~/Content/noImg.png");
                }
            }
            catch
            {
                return Redirect("~/Content/noImg.png");
            }
            
            
        }

    }
}
