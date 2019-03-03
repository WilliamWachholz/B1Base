using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class DiscountDAO
    {
        public void Save(int absEntry, List<Model.DiscountGroupModel> discountGroupList)
        {            
            CompanyService companyService = Controller.ConnectionController.Instance.Company.GetCompanyService();
            EnhancedDiscountGroupsService enhancedDiscountGroupsService = companyService.GetBusinessService(ServiceTypes.EnhancedDiscountGroupsService);

            EnhancedDiscountGroupParams enhancedDiscountGroupsParams = enhancedDiscountGroupsService.GetDataInterface(EnhancedDiscountGroupsServiceDataInterfaces.edgsEnhancedDiscountGroupParams);
            enhancedDiscountGroupsParams.AbsEntry = absEntry;
            
            EnhancedDiscountGroup enchancedDiscountGroup = enhancedDiscountGroupsService.Get(enhancedDiscountGroupsParams);
            
            try
            {
                for (int row = enchancedDiscountGroup.DiscountGroupLineCollection.Count - 1; row >= 0; row--) 
                {
                    DiscountGroupLine discountGroupLine = enchancedDiscountGroup.DiscountGroupLineCollection.Item(row);

                    Model.DiscountGroupModel discountGroupModel = null;

                    if (discountGroupLine.ObjectType == SAPbobsCOM.DiscountGroupBaseObjectEnum.dgboItemGroups)
                    {
                        if (discountGroupList.Exists(r => r.ItemGroup.ToString() == discountGroupLine.ObjectCode))
                        {
                            discountGroupModel = discountGroupList.First(r => r.ItemGroup.ToString() == discountGroupLine.ObjectCode);
                        }
                    }
                    else if (discountGroupLine.ObjectType == SAPbobsCOM.DiscountGroupBaseObjectEnum.dgboItems)
                    {
                        if (discountGroupList.Exists(r => r.ItemCode == discountGroupLine.ObjectCode))
                        {
                            discountGroupModel = discountGroupList.First(r => r.ItemCode == discountGroupLine.ObjectCode);
                        }
                    }

                    if (discountGroupModel != null)
                    {
                        discountGroupModel.Insert = false;

                        if (discountGroupModel.Delete)
                        {
                            enchancedDiscountGroup.DiscountGroupLineCollection.Remove(row);
                        }
                        else
                        {
                            discountGroupLine.Discount = discountGroupModel.Discount;
                        }
                    }
                }

                foreach (Model.DiscountGroupModel discountGroupModel in discountGroupList.Where(r => r.Insert && !r.Delete).ToList())
                {
                    enchancedDiscountGroup.DiscountGroupLineCollection.Add();

                    DiscountGroupLine discountGroupLine = enchancedDiscountGroup.DiscountGroupLineCollection.Item(enchancedDiscountGroup.DiscountGroupLineCollection.Count - 1);

                    if (discountGroupModel.ItemCode != string.Empty)
                    {
                        discountGroupLine.ObjectType = DiscountGroupBaseObjectEnum.dgboItems;
                        discountGroupLine.ObjectCode = discountGroupModel.ItemCode;
                    }
                    else if (discountGroupModel.ItemGroup != 0)
                    {
                        discountGroupLine.ObjectType = DiscountGroupBaseObjectEnum.dgboItemGroups;
                        discountGroupLine.ObjectCode = discountGroupModel.ItemGroup.ToString();
                    }

                    discountGroupLine.Discount = discountGroupModel.Discount;
                }                

                enhancedDiscountGroupsService.Update(enchancedDiscountGroup);

                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
            }
            finally
            {
                Marshal.ReleaseComObject(companyService);
                Marshal.ReleaseComObject(enhancedDiscountGroupsService);
                Marshal.ReleaseComObject(enhancedDiscountGroupsParams);
                Marshal.ReleaseComObject(enchancedDiscountGroup);

                GC.Collect();
            }
        }
    }
}
