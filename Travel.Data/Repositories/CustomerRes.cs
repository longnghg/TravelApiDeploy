﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Travel.Data.Interfaces;
using Travel.Shared.ViewModels;
using Travel.Shared.ViewModels.Travel.CustomerVM;
using Travel.Shared.Ultilities;
using PrUtility;
using Travel.Context.Models.Travel;
using Travel.Context.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace Travel.Data.Repositories
{
    public class CustomerRes : ICustomer
    {
        private readonly TravelContext _db;
        private Notification message;
        private readonly IConfiguration _config;
        private Response res;
        private readonly ILog _log;
        public CustomerRes(TravelContext db, IConfiguration config, ILog log)
        {
            _db = db;
            message = new Notification();
            _config = config;
            res = new Response();
            _log = log;
        }
        private void UpdateDatabase<T>(T input)
        {
            _db.Entry(input).State = EntityState.Modified;
            _db.SaveChanges();
        }
        private void DeleteDatabase<T>(T input)
        {
            _db.Entry(input).State = EntityState.Deleted;
            _db.SaveChanges();
        }
        private void CreateDatabase<T>(T input)
        {
            _db.Entry(input).State = EntityState.Added;
            _db.SaveChanges();
        }

        private async Task SaveChangeAsync()
        {
            await _db.SaveChangesAsync();
        }
        private async Task<List<TourBooking>> CallServiceTourBookingByIdCustomer(Guid idCustomer)
        {
            using (var client = new HttpClient())
            {
                var urlService = _config["UrlService"].ToString();
                client.BaseAddress = new Uri($"{urlService}");
                client.DefaultRequestHeaders.Accept.Clear();
                HttpResponseMessage response = await client.GetAsync($"api/tourbooking/list-tour-booking-by-id-customer-s?idCustomer={idCustomer}");
                if (response.IsSuccessStatusCode)
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    string data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TourBooking>>(data, options);
                }

            }
            return null;
        }
        public async Task<Response> CustomerSendRate(string idTour, int rating)
        {
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                await transaction.CreateSavepointAsync("BeforeSave");

                var tour = await (from x in _db.Tour.AsNoTracking()
                                  where x.IdTour == idTour
                                  select x).FirstOrDefaultAsync();

                var listReviewByTour = await (from x in _db.reviews.AsNoTracking()
                                              where x.IdTour == idTour
                                              select x).ToListAsync();
                // create review0
                var review = new Review()
                {
                    Id = Guid.NewGuid(),
                    Rating = rating,
                    IdTour = idTour
                };
                listReviewByTour.Add(review);
                CreateDatabase(review);
                tour.Rating = listReviewByTour.Average(x => x.Rating);
                UpdateDatabase(tour);
                await SaveChangeAsync();

                transaction.Commit();
                transaction.Dispose();

                return Ultility.Responses("Cảm ơn bạn đã đánh giá !", Enums.TypeCRUD.Success.ToString());

            }
            catch (Exception e)
            {
                transaction.RollbackToSavepoint("BeforeSave");
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }

        public string CheckBeforeSave(JObject frmData, ref Notification _message, bool isUpdate)
        {
            try
            {
                if (frmData != null)
                {
                    var idCustomer = PrCommon.GetString("idCustomer", frmData);
                    var nameCustomer = PrCommon.GetString("nameCustomer", frmData);
                    if (String.IsNullOrEmpty(nameCustomer))
                    {
                    }
                    var phone = PrCommon.GetString("phone", frmData);
                    var email = PrCommon.GetString("email", frmData);


                    #region validation

                    if (isUpdate)
                    {
                        if (!String.IsNullOrEmpty(phone))
                        {
                            var check = CheckPhoneCustomer(phone, idCustomer);
                            if (check.Notification.Type == "Validation" || check.Notification.Type == "Error")
                            {
                                _message = check.Notification;
                                return string.Empty;
                            }
                        }
                        if (!String.IsNullOrEmpty(email))
                        {
                            var check = CheckEmailCustomer(email, idCustomer);
                            if (check.Notification.Type == "Validation" || check.Notification.Type == "Error")
                            {
                                _message = check.Notification;
                                return string.Empty;
                            }
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(phone))
                        {
                            var check = CheckPhoneCustomer(phone);
                            if (check.Notification.Type == "Validation" || check.Notification.Type == "Error")
                            {
                                _message = check.Notification;
                                return string.Empty;
                            }
                        }
                        if (!String.IsNullOrEmpty(email))
                        {
                            var check = CheckEmailCustomer(email);
                            if (check.Notification.Type == "Validation" || check.Notification.Type == "Error")
                            {
                                _message = check.Notification;
                                return string.Empty;
                            }
                        }
                    }
                 
                    
                    #endregion

                    var birthday = PrCommon.GetString("birthday", frmData);
                    if (String.IsNullOrEmpty(birthday))
                    {

                    }


                    var address = PrCommon.GetString("address", frmData);
                    if (String.IsNullOrEmpty(address))
                    {
                    }

                    var password = PrCommon.GetString("password", frmData);
                    if (String.IsNullOrEmpty(password))
                    {
                    }

                    var gender = PrCommon.GetString("gender", frmData);
                    if (String.IsNullOrEmpty(gender))
                    {
                    }

                    var modifyBy = PrCommon.GetString("modifyBy", frmData);
                    if (String.IsNullOrEmpty(modifyBy))
                    {
                    }

                    if (isUpdate)
                    {
                        UpdateCustomerViewModel objUpdate = new UpdateCustomerViewModel();
                        objUpdate.IdCustomer = Guid.Parse(idCustomer);
                        objUpdate.NameCustomer = nameCustomer;
                        objUpdate.Phone = phone;
                        objUpdate.Email = email;
                        objUpdate.Address = address;
                        if (birthday != "0" && !string.IsNullOrEmpty(birthday))
                        {
                            objUpdate.Birthday = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Parse(birthday));
                        }
                        objUpdate.Gender = Convert.ToBoolean(gender);
                        return JsonSerializer.Serialize(objUpdate);
                    }
                    CreateCustomerViewModel objCreate = new CreateCustomerViewModel();
                    objCreate.NameCustomer = nameCustomer;
                    objCreate.Phone = phone;
                    objCreate.Email = email;
                    objCreate.Address = address;
                    //objCreate.Birthday = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(DateTime.Parse(birthday));
                    objCreate.Password = Ultility.Encryption(password);
                    return JsonSerializer.Serialize(objCreate);
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                _message = Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message).Notification;
                return string.Empty;
            }
        }

        public Response Create(CreateCustomerViewModel input)
        {
            try
            {
                Customer customer = Mapper.MapCreateCustomer(input);
                customer.IdCustomer = Guid.NewGuid();
                customer.Point = 0;
                customer.IsDelete = false;
                string jsonContent = JsonSerializer.Serialize(customer);
                CreateDatabase(customer);

                return Ultility.Responses("Đăng ký thành công !", Enums.TypeCRUD.Success.ToString());

            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);

            }
        }

        public Response Gets()
        {
            try
            {
                var queryListCus = (from x in _db.Customers.AsNoTracking()
                                    where x.IsDelete == false
                                    select x);


                int totalResult = queryListCus.Count();
                var listCus = queryListCus.ToList();
                var result = Mapper.MapCustomer(listCus);
                var res = Ultility.Responses("", Enums.TypeCRUD.Success.ToString(), result);
                res.TotalResult = totalResult;
                return res;



            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }
        public async Task<Response> GetsHistory(Guid idCustomer)
        {
            try
            {
                var listTourBookingByIdCustomer = await CallServiceTourBookingByIdCustomer(idCustomer);
                var list = (from x in listTourBookingByIdCustomer
                            where x.CustomerId == idCustomer
                            orderby x.DateBooking descending
                            select new TourBooking
                            {
                                IdTourBooking = x.IdTourBooking,
                                Status = x.Status,
                                TotalPrice = x.TotalPrice,
                                TotalPricePromotion = x.TotalPricePromotion,
                                ScheduleId = x.ScheduleId,
                                DateBooking = x.DateBooking,
                                BookingNo = x.BookingNo,
                                ValuePromotion = x.ValuePromotion,
                                IsSendFeedBack = x.IsSendFeedBack,
                                TourBookingDetails = x.TourBookingDetails,
                                Schedule = (from s in _db.Schedules.AsNoTracking()
                                            where x.ScheduleId == s.IdSchedule
                                            select new Schedule
                                            {
                                                Description = s.Description,
                                                DepartureDate = s.DepartureDate,
                                                DeparturePlace = s.DeparturePlace,
                                                ReturnDate = s.ReturnDate,
                                                Tour = (from t in _db.Tour.AsNoTracking()
                                                        where s.TourId == t.IdTour
                                                        select new Tour
                                                        {
                                                            Thumbnail = t.Thumbnail,
                                                            NameTour = t.NameTour,
                                                            ToPlace = t.ToPlace
                                                        }).First()
                                            }).First()
                            }).ToList();

                var result = Mapper.MapHistoryCustomerViewModel(list);

                if (result.Count() > 0)
                {
                    return Ultility.Responses("", Enums.TypeCRUD.Success.ToString(), result);
                }
                else
                {
                    return Ultility.Responses("", Enums.TypeCRUD.Warning.ToString(), null);
                }
                //var list = (from x in _db.Tourbookings
                //            where x.CustomerId == idCustomer
                //            select new Tourbooking {
                //                ValuePromotion = x.ValuePromotion,
                //                CustomerId = x.CustomerId,
                //                IsCalled = x.IsCalled,
                //                NameContact = x.NameContact,
                //                NameCustomer = x.NameCustomer,
                //                DateBooking = x.DateBooking,
                //                Deposit = x.Deposit,
                //                VoucherCode = x.VoucherCode,
                //                Address = x.Address,
                //                LastDate = x.LastDate,
                //                ModifyDate = x.ModifyDate,
                //                BookingNo = x.BookingNo,
                //                Email = x.Email,
                //                IdTourbooking = x.IdTourbooking,
                //                ModifyBy = x.ModifyBy,

                //                PaymentId = x.PaymentId,
                //                Phone = x.Phone,
                //                Pincode = x.Pincode,
                //                RemainPrice = x.RemainPrice,
                //                ScheduleId = x.ScheduleId,
                //                TotalPrice = x.TotalPrice,
                //                TotalPricePromotion = x.TotalPricePromotion,
                //                Vat = x.Vat,
                //                Payment = (from p in _db.Payment where p.IdPayment == x.PaymentId select p).First(),
                //                Schedule = (from s in _db.Schedules where s.IdSchedule == x.ScheduleId
                //                            select new Schedule{ 
                //                DepartureDate = s.DepartureDate,
                //                Tour = (from t  in _db.Tour where t.IdTour == s.TourId select t).First()
                //                }).First(),
                //                TourbookingDetails = (from td in _db.tourbookingDetails where td.IdTourbookingDetails == x.IdTourbooking select td).First()
                //            }).ToList();
                //var result = Mapper.MapTourBooking(list);

                //if (list.Count() > 0)
                //{
                //    res.Content = result;
                //}
                //return res;
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }



        public async Task<Response> SendOTP(string email)
        {
            try
            {
                var account = await (from x in _db.Customers.AsNoTracking()
                                     where x.Email.ToLower() == email.ToLower()
                                     select x).FirstOrDefaultAsync();
                if (account != null)
                {
                    string otpCode = Ultility.RandomString(8, false);
                    OTP obj = new OTP();
                    var dateTime = DateTime.Now;
                    var begin = dateTime;
                    var end = dateTime.AddMinutes(2);
                    obj.BeginTime = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(begin);
                    obj.EndTime = Ultility.ConvertDatetimeToUnixTimeStampMiliSecond(end);
                    obj.OTPCode = otpCode;

                    var subjectOTP = _config["OTPSubject"];
                    var emailSend = _config["emailSend"];
                    var keySecurity = _config["keySecurity"];
                    var stringHtml = Ultility.getHtml(otpCode, subjectOTP, "OTP");



                    Ultility.sendEmail(stringHtml, email, "Yêu cầu quên mật khẩu", emailSend, keySecurity);







                    return Ultility.Responses($"Mã OTP đã gửi vào email {email}!", Enums.TypeCRUD.Success.ToString(), obj);

                }
                else
                {
                    return Ultility.Responses($"{email} không tồn tại!", Enums.TypeCRUD.Error.ToString());
                }
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }
        public async Task<Response> GetCustomer(Guid idCustomer)
        {
            try
            {
                var customer = await (from x in _db.Customers.AsNoTracking()
                                where x.IdCustomer == idCustomer
                                select x).FirstOrDefaultAsync();
                var result = Mapper.MapCustomer(customer);
                if (result != null)
                {
                    return Ultility.Responses("", Enums.TypeCRUD.Success.ToString(), result);
                }
                else
                {
                    return Ultility.Responses("", Enums.TypeCRUD.Warning.ToString());
                }
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }

        public async Task<Response> UpdateCustomer(UpdateCustomerViewModel input)
        {
            try
            {
                var customer = await (from x in _db.Customers.AsNoTracking()
                                      where x.IdCustomer == input.IdCustomer
                                      select x).FirstOrDefaultAsync();

                customer.NameCustomer = input.NameCustomer;
                customer.Phone = input.Phone;
                customer.Email = input.Email;
                customer.Address = input.Address;
                customer.Gender = input.Gender;
                customer.Birthday = input.Birthday;
                string jsonContent = JsonSerializer.Serialize(customer);
                UpdateDatabase(customer);

                return Ultility.Responses("Cập nhật thành công !", Enums.TypeCRUD.Success.ToString());
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);
            }
        }



        public async Task<bool> UpdateScoreToCustomer(Guid idCustomer, int point)
        {
            try
            {
                var customer = await (from x in _db.Customers.AsNoTracking()
                                      where x.IdCustomer == idCustomer
                                      select x).FirstOrDefaultAsync();
                if (customer != null)
                {
                    customer.Point += point;

                    customer.Legit += 10;
                    if (customer.Legit > 100)
                    {
                        customer.Legit = 100;
                    }
                    UpdateDatabase(customer);
                    await SaveChangeAsync();
                    return true;

                }
                return false;
            }
            catch
            {
                return false;

            }
        }

        public async Task<Response> UpdateBlockCustomer(Guid idCustomer, bool isBlock)
        {
            try
            {
                var customer = await (from x in _db.Customers.AsNoTracking()
                                      where x.IdCustomer == idCustomer
                                      select x).FirstOrDefaultAsync();
                if (customer != null)
                {
                    if (isBlock)
                    {
                        customer.IsBlock = false;
                    }
                    else
                    {
                        customer.IsBlock = true;
                    }

                    _db.Customers.Update(customer);
                    //UpdateDatabase(customer);
                    await SaveChangeAsync();
                }
                return Ultility.Responses("Thay đổi trạng thái thành công !", Enums.TypeCRUD.Success.ToString());
            }
            catch (Exception e)
            {
                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);

            }
        }

        public Response Search(JObject frmData)
        {
            try
            {
                var totalResult = 0;
                Keywords keywords = new Keywords();
                var pageSize = PrCommon.GetString("pageSize", frmData) == null ? 10 : Convert.ToInt16(PrCommon.GetString("pageSize", frmData));
                var pageIndex = PrCommon.GetString("pageIndex", frmData) == null ? 1 : Convert.ToInt16(PrCommon.GetString("pageIndex", frmData));

                var isBlock = PrCommon.GetString("isDelete", frmData);
                if (!String.IsNullOrEmpty(isBlock))
                {
                    keywords.IsBlock = Boolean.Parse(isBlock);
                }

                var kwNameCustomer = PrCommon.GetString("nameCustomer", frmData);
                if (!String.IsNullOrEmpty(kwNameCustomer))
                {
                    keywords.KwName = kwNameCustomer;
                }
                else
                {
                    keywords.KwName = "";
                }

                var kwEmail = PrCommon.GetString("email", frmData);
                if (!String.IsNullOrEmpty(kwEmail))
                {
                    keywords.KwEmail = kwEmail;
                }
                else
                {
                    keywords.KwEmail = "";
                }

                var kwPhone = PrCommon.GetString("phone", frmData);
                if (!String.IsNullOrEmpty(kwPhone))
                {
                    keywords.KwPhone = kwPhone;
                }
                else
                {
                    keywords.KwPhone = "";
                }

                var kwAddress = PrCommon.GetString("address", frmData);
                if (!String.IsNullOrEmpty(kwAddress))
                {
                    keywords.KwAddress = kwAddress;
                }
                else
                {
                    keywords.KwAddress = "";
                }

                var kwPoint = PrCommon.GetString("point", frmData);
                if (!String.IsNullOrEmpty(kwPoint))
                {
                    keywords.KwPoint = int.Parse(kwPoint);
                }
                else
                {
                    keywords.KwPoint = 0;
                }

                var listCustomer = new List<Customer>();

                if (keywords.KwPoint > 0)
                {
                    var querylistCustomer = (from c in _db.Customers
                                             where c.IsBlock == keywords.IsBlock &&
                                                   c.NameCustomer.ToLower().Contains(keywords.KwName) &&
                                                   c.Phone.ToLower().Contains(keywords.KwPhone) &&
                                                   c.Email.ToLower().Contains(keywords.KwEmail) &&
                                                   c.Address.ToLower().Contains(keywords.KwAddress) &&
                                                   c.Point == keywords.KwPoint
                                             select c).ToList();
                    totalResult = querylistCustomer.Count();
                    listCustomer = querylistCustomer.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                }
                else
                {
                    var querylistCustomer = (from c in _db.Customers
                                             where c.IsBlock == keywords.IsBlock &&
                                                   c.NameCustomer.ToLower().Contains(keywords.KwName) &&
                                                   c.Phone.ToLower().Contains(keywords.KwPhone) &&
                                                   c.Email.ToLower().Contains(keywords.KwEmail) &&
                                                   c.Address.ToLower().Contains(keywords.KwAddress)
                                             select c).ToList();
                    totalResult = querylistCustomer.Count();
                    listCustomer = querylistCustomer.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
                }


                var result = Mapper.MapCustomer(listCustomer);
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








        #region check same

        public Response CheckEmailCustomer(string email,string idCustomer = null)
        {
            try
            {
                // update
                if (!string.IsNullOrEmpty(idCustomer))
                {
                    Guid id = Guid.Parse(idCustomer);
                    string oldEmail = (from x in _db.Customers.AsNoTracking()
                                       where x.IdCustomer == id
                                       select x).FirstOrDefault().Email;
                    if (email != oldEmail) // có thay đổi sdt
                    {
                        var obj = (from x in _db.Employees where x.Email != oldEmail && x.Email == email select x).Count();
                        if (obj > 0)
                        {
                            return Ultility.Responses("[" + email + "] này đã được đăng ký !", Enums.TypeCRUD.Validation.ToString(), description: "email");
                        }
                    }
                }
                else
                {
                    var emp = (from x in _db.Customers.AsNoTracking()
                               where x.IsDelete == false && x.Email == email
                               select x).Count();
                    if (emp > 0)
                    {
                        return Ultility.Responses("[" + email + "] này đã được đăng ký !", Enums.TypeCRUD.Validation.ToString(), description: "email");
                    }
                }
                return res;


            }
            catch (Exception e)
            {

                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);

            }
        }

        public Response CheckPhoneCustomer(string phone, string idCustomer = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(idCustomer)) // update
                {
                    Guid id = Guid.Parse(idCustomer);
                    string oldPhone = (from x in _db.Customers.AsNoTracking()
                                       where x.IdCustomer == id
                                       select x).First().Phone;
                    if (phone != oldPhone) // có thay đổi  sdt
                    {
                        var obj = (from x in _db.Customers where x.Phone != oldPhone && x.Phone == phone select x).Count();
                        if (obj > 0)
                        {
                            return Ultility.Responses("[" + phone + "] này đã được đăng ký !", Enums.TypeCRUD.Validation.ToString());
                        }
                    }
                }
                else // create
                {
                    var emp = (from x in _db.Customers where x.Phone == phone select x).Count();
                    if (emp > 0)
                    {
                        return Ultility.Responses("[" + phone + "] này đã được đăng ký !", Enums.TypeCRUD.Validation.ToString());
                    }
                }
                return res;

            }
            catch (Exception e)
            {

                return Ultility.Responses("Có lỗi xảy ra !", Enums.TypeCRUD.Error.ToString(), description: e.Message);

            }
        }


        #endregion



        public async Task<Guid> GetCustomerIdByPhone(string phone)
        {
            var customer = await (from x in _db.Customers.AsNoTracking()
                            where x.Phone == phone
                            select x.IdCustomer).FirstOrDefaultAsync();
            return customer;
        }
    }


}
