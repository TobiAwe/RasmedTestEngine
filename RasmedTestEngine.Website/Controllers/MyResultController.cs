using System.Linq;
using System.Web.Mvc;
using RasmedTestEngine.Core.Abstract;

namespace RasmedTestEngine.Website.Controllers
{
    [Authorize]
    public class MyResultController : Controller
    {
        private readonly IExaminationRepository _examinationRepository;

       
        public MyResultController(IExaminationRepository examinationRepository)
        {
            _examinationRepository = examinationRepository;
        }

        // GET: MyResult
        public ActionResult Index()
        {
            var me = User.Identity.Name;
            var allResults =
                _examinationRepository.GetResultManagers.Where(
                    x => x.MemberProfile.Member.UserName == me && x.Status == "Finished")
                    .OrderByDescending(x => x.StarTime)
                    .ToList();
            return View(allResults);
        }
    }
}