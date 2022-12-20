﻿
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Travel.Context.Models;
using Travel.Context.Models.Travel;
using Travel.Data.Interfaces;
using Travel.Shared.Ultilities;
using Travel.Shared.ViewModels;
using Travel.Shared.ViewModels.Travel.VoucherVM;

namespace Travel.Data.Repositories
{
    public class VoucherRes : IVoucher
    {
        private readonly TravelContext _db;
        private Notification message;
        private Response res;
        private readonly ILog _log;
        public VoucherRes(TravelContext db , ILog log)
        {
            _db = db;
            _log = log;
            message = new Notification();
            res = new Response();
        }
        public string CheckBeforSave(JObject frmData, ref Notification _message, bool isUpdate)
        {
            try
            {            
                var code = PrCommon.GetString("code", frmData);
                if (String.IsNullOrEmpty(code))
                {
                }
             
                var value = PrCommon.GetString("value", frmData);
                if (String.IsNullOrEmpty(value))
                {
                }
                var startDate = PrCommon.GetString("startDate", frmData);
                if (String.IsNullOrEmpty(startDate))
                {
                }
                var endDate = PrCommon.GetString("endDate", frmData);
                if (String.IsNullOrEmpty(endDate))
                {
                }
               
              
                if (isUpdate)
                {
                    // map data
                    UpdateVoucherViewModel objUpdate = new UpdateVoucherViewModel();
                     objUpdate.Code = code;
                    objUpdate.Value = int.Parse(value);
                    objUpdate.StartDate = long.Parse(startDate);
                    objUpdate.EndDate = long.Parse(endDate);
                   
                    // generate ID

                    return JsonSerializer.Serialize(objUpdate);
                }
                // map data
                CreateVoucherViewModel obj = new CreateVoucherViewModel();
                
                obj.Code = Ultility.RandomString(8, false);
                obj.Value = int.Parse(value);
                obj.StartDate = long.Parse(startDate);
                obj.EndDate = long.Parse(endDate);
             
                return JsonSerializer.Serialize(obj);
            }
            catch (Exception e)
            {
                message.DateTime = DateTime.Now;
                message.Description = e.Message;
                message.Messenge = "Có lỗi xảy ra !";
                message.Type = "Error";
                _message = message;
                return null;
            }
        }

        public Response CreateTiket(Guid idVoucher, Guid idCus)
        {
           
            try
            {
                
                var cus = _db.Customers.Find(idCus);
                var vou = _db.Vouchers.Find(idVoucher);
                var voucher = new Customer_Voucher();

                if (cus.Point > vou.Value)
                {
                    var value = cus.Point - vou.Value;
                    cus.Point = value;
                    voucher.VoucherId = idVoucher;
                    voucher.CustomerId = idCus;              
                    _db.Customer_Vouchers.Add(voucher);
                    _db.SaveChanges();
                    return Ultility.Responses("Mua thành công !", Enums.TypeCRUD.Success.ToString());
                }
                else
                {
                    return Ultility.Responses("Bạn không đủ điểm  !", Enums.TypeCRUD.Success.ToString());
                }
                
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }

        public Response CreateVoucher(CreateVoucherViewModel input, string emailUser)
        {
            try
            {
                Voucher voucher = new Voucher();
                voucher = Mapper.MapCreateVoucher(input);
                string jsonContent = JsonSerializer.Serialize(voucher);

                _db.Vouchers.Add(voucher);
                _db.SaveChanges();
                bool result = _log.AddLog(content: jsonContent, type: "create", emailCreator: emailUser, classContent: "Voucher");
                if (result)
                {
                    return Ultility.Responses($"Thêm thành công !", Enums.TypeCRUD.Success.ToString());
                }
                else
                {
                    return Ultility.Responses("Lỗi log!", Enums.TypeCRUD.Error.ToString());
                }
            }
            catch (Exception e)
            {

                res.Notification.DateTime = DateTime.Now;
                res.Notification.Description = e.Message;
                res.Notification.Messenge = "Có lỗi xảy ra !";
                res.Notification.Type = "Error";
                return res;
            }
        }

        public Response DeleteVoucher(Guid id, string emailUser)
        {
            try
            {
                var voucher = _db.Vouchers.Find(id);
                if (voucher != null)
                {
                    string jsonContent = JsonSerializer.Serialize(voucher);
                    _db.Vouchers.Remove(voucher);
                    _db.SaveChanges();

                    bool result = _log.AddLog(content: jsonContent, type: "delete", emailCreator: emailUser, classContent: "Voucher");
                    if (result)
                    {
                        return Ultility.Responses($"Xóa thành công !", Enums.TypeCRUD.Success.ToString());
                    }
                    else
                    {
                        return Ultility.Responses("Lỗi log!", Enums.TypeCRUD.Error.ToString());
                    }

                }
                else
                {
                    return Ultility.Responses($"Không tìm thấy !", Enums.TypeCRUD.Warning.ToString());

                }
            }
            catch (Exception e)
            {
                res.Notification.DateTime = DateTime.Now;
                res.Notification.Description = e.Message;
                res.Notification.Messenge = "Có lỗi xảy ra !";
                res.Notification.Type = "Error";
                return res;
            }
        }

        public Response GetsVoucher(bool isDelete)
        {
            try
            {
                int addMinutes = 0;
                var dateNow = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Now.AddMinutes(addMinutes));
                var list = (from x in _db.Vouchers.AsNoTracking()
                            where x.StartDate <=  dateNow &&
                                  x.EndDate >= dateNow
                            select x).ToList();
                var result = Mapper.MapVoucher(list);
                return Ultility.Responses("", Enums.TypeCRUD.Success.ToString(), result);
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }

        public Response RestoreVoucher(Guid id, string emailUser)
        {
            try
            {
                var voucher = _db.Vouchers.Find(id);
                if (voucher != null)
                {
                    string jsonContent = JsonSerializer.Serialize(voucher);

                    _db.SaveChanges();

                    bool result = _log.AddLog(content: jsonContent, type: "restore", emailCreator: emailUser, classContent: "Voucher");
                    if (result)
                    {
                        return Ultility.Responses($"Khôi phục thành công !", Enums.TypeCRUD.Success.ToString());
                    }
                    else
                    {
                        return Ultility.Responses("Lỗi log!", Enums.TypeCRUD.Error.ToString());
                    }
                }
                else
                {
                    res.Notification.DateTime = DateTime.Now;
                    res.Notification.Messenge = "Không tìm thấy !";
                    res.Notification.Type = "Warning";
                }
                return res;
            }
            catch (Exception e)
            {
                res.Notification.DateTime = DateTime.Now;
                res.Notification.Description = e.Message;
                res.Notification.Messenge = "Có lỗi xảy ra !";
                res.Notification.Type = "Error";
                return res;
            }
        }

        public Response UpdateVoucher(UpdateVoucherViewModel input,string emailUser)
        {
            try
            {
                var update = (from x in _db.Vouchers where x.IdVoucher == input.IdVoucher select x).FirstOrDefault();
                Voucher voucher = new Voucher();
                string jsonContent = JsonSerializer.Serialize(voucher);

                voucher = Mapper.MapUpdateVoucher(input);
                _db.Vouchers.Update(voucher);
                _db.SaveChanges();

              
                bool result = _log.AddLog(content: jsonContent, type: "update", emailCreator: emailUser, classContent: "Voucher");
                if (result)
                {
                    return Ultility.Responses($"Sửa thành công !", Enums.TypeCRUD.Success.ToString());
                }
                else
                {
                    return Ultility.Responses("Lỗi log!", Enums.TypeCRUD.Error.ToString());
                }
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);

            }
        }

        public Response GetsVoucherHistory(Guid idCustomer)
        {
            try
            {
                int addMinutes = 0;
                var dateNow = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Now.AddMinutes(addMinutes));
                var list = (from x in _db.Customer_Vouchers
                            join v in _db.Vouchers on x.VoucherId equals v.IdVoucher
                            where v.StartDate <= dateNow &&
                                  v.EndDate >= dateNow
                            select v).ToList();
    
                return Ultility.Responses("", Enums.TypeCRUD.Success.ToString(), list);
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }


        #region service call
        public async Task<Voucher> CheckIsVoucherValid(string code,Guid customerId)
        {

            var unixDateTimeNow = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Now);
            var vourcher = await (from x in _db.Vouchers.AsNoTracking()
                                  join vc in _db.Customer_Vouchers.AsNoTracking()
                                  on x.IdVoucher equals vc.VoucherId
                              where x.Code == code
                              && vc.CustomerId == customerId
                              && x.EndDate >= unixDateTimeNow
                              select x).FirstOrDefaultAsync();
            return vourcher;
        }

        public async Task DeleteVourcherCustomer(Guid idVoucher)
        {
            var voucherCus = await (from x in _db.Customer_Vouchers
                                    where x.VoucherId == idVoucher
                                    select x).FirstOrDefaultAsync();
            _db.Customer_Vouchers.Remove(voucherCus);
            await _db.SaveChangesAsync();
        }
        #endregion

        public Response SearchVoucher(JObject frmData)
        {
            try
            {
                var totalResult = 0;
                Keywords keywords = new Keywords();
                var pageSize = PrCommon.GetString("pageSize", frmData) == null ? 10 : Convert.ToInt16(PrCommon.GetString("pageSize", frmData));
                var pageIndex = PrCommon.GetString("pageIndex", frmData) == null ? 1 : Convert.ToInt16(PrCommon.GetString("pageIndex", frmData));

                var isDelete = PrCommon.GetString("isDelete", frmData);
                if (!String.IsNullOrEmpty(isDelete))
                {
                    keywords.IsDelete = Boolean.Parse(isDelete);
                }
                var kwCode = PrCommon.GetString("code", frmData);
                if (!String.IsNullOrEmpty(kwCode))
                {
                    keywords.KwCode = kwCode.Trim().ToLower();
                }
                else
                {
                    keywords.KwCode = "";
                }

                var kwValue = PrCommon.GetString("value", frmData);
                if (!String.IsNullOrEmpty(kwValue))
                {
                    keywords.KwValue = int.Parse(kwValue);
                }
                else
                {
                    keywords.KwValue = 0;
                }

                var kwBeginDate = PrCommon.GetString("startDate", frmData);
                if (!String.IsNullOrEmpty(kwBeginDate))
                {
                    keywords.KwBeginDate = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Parse(kwBeginDate));
                }
                else
                {
                    keywords.KwBeginDate = 0;
                }

                var kwEndDate = PrCommon.GetString("endDate", frmData);
                if (!String.IsNullOrEmpty(kwEndDate))
                {
                    keywords.KwEndDate = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Parse(kwEndDate).AddDays(1).AddSeconds(-1));
                }
                else
                {
                    keywords.KwEndDate = 0;
                }
                var listVoucher = new List<Voucher>();

                #region querylistVoucher
                if (keywords.KwBeginDate > 0 || keywords.KwEndDate > 0)
                {
                    if(keywords.KwValue > 0)
                    {
                        if (keywords.KwBeginDate > 0 && keywords.KwEndDate > 0)
                        {
                            var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                    where x.Code.ToLower().Contains(keywords.KwCode) &&
                                                          x.Value == keywords.KwValue &&
                                                          x.StartDate >= keywords.KwBeginDate &&
                                                          x.EndDate <= keywords.KwEndDate
                                                    select x);
                            totalResult = querylistVoucher.Count();
                            listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        }
                        else if (keywords.KwBeginDate == 0 && keywords.KwEndDate > 0)
                        {
                            var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                    where x.Code.ToLower().Contains(keywords.KwCode) &&
                                                          x.Value == keywords.KwValue &&
                                                          x.EndDate <= keywords.KwEndDate
                                                    select x);
                            totalResult = querylistVoucher.Count();
                            listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        }
                        else if (keywords.KwBeginDate > 0 && keywords.KwEndDate == 0)
                        {
                            var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                    where x.Code.ToLower().Contains(keywords.KwCode) &&
                                                          x.Value == keywords.KwValue &&
                                                    x.StartDate >= keywords.KwBeginDate
                                                    select x);
                            totalResult = querylistVoucher.Count();
                            listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        }
                    }
                    else
                    {
                        if (keywords.KwBeginDate > 0 && keywords.KwEndDate > 0)
                        {
                            var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                    where x.Code.ToLower().Contains(keywords.KwCode) &&
                                                          x.StartDate >= keywords.KwBeginDate &&
                                                          x.EndDate <= keywords.KwEndDate
                                                    select x);
                            totalResult = querylistVoucher.Count();
                            listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        }
                        else if (keywords.KwBeginDate == 0 && keywords.KwEndDate > 0)
                        {
                            var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                    where x.Code.ToLower().Contains(keywords.KwCode) &&
                                                          x.EndDate <= keywords.KwEndDate
                                                    select x);
                            totalResult = querylistVoucher.Count();
                            listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        }
                        else if (keywords.KwBeginDate > 0 && keywords.KwEndDate == 0)
                        {
                            var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                    where x.Code.ToLower().Contains(keywords.KwCode) &&
                                                    x.StartDate >= keywords.KwBeginDate
                                                    select x);
                            totalResult = querylistVoucher.Count();
                            listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                        }
                    }
                }
                else
                {
                    if (keywords.KwValue > 0)
                    {
                        var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                where x.Value == keywords.KwValue &&
                                                x.Code.ToLower().Contains(keywords.KwCode)
                                                select x);
                        totalResult = querylistVoucher.Count();
                        listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                    }
                    else
                    {
                        var querylistVoucher = (from x in _db.Vouchers.AsNoTracking()
                                                where x.Code.ToLower().Contains(keywords.KwCode)
                                                select x);
                        totalResult = querylistVoucher.Count();
                        listVoucher = querylistVoucher.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                    }

                }

                #endregion
                
                
                var result = Mapper.MapVoucher(listVoucher);
                if (result.Count() > 0)
                {
                    var res = Ultility.Responses("", Enums.TypeCRUD.Success.ToString(), result);
                    res.TotalResult = totalResult;
                    return res;
                }
                else
                {
                    return Ultility.Responses($"Không có dữ liệu trả về !", Enums.TypeCRUD.Warning.ToString());
                }


            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }
    }
}
