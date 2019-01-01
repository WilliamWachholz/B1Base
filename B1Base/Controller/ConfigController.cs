using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Controller
{
    class ConfigController
    {
        DAO.ConfigDAO _configDAO;

        public ConfigController()
        {
            _configDAO = new DAO.ConfigDAO();
        }

        public Model.ConfigModel GetConfig(int code)
        {
            try
            {
                return _configDAO.Get(code);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }

            return new Model.ConfigModel();
        }

        public void SaveConfig(Model.ConfigModel configModel)
        {
            try
            {
                _configDAO.Save(configModel);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }
        }
    }
}
