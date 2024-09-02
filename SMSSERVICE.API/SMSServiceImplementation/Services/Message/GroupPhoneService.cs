using AutoMapper;
using AutoMapper.QueryableExtensions;
using Implementation.Helper;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using SMSServiceInfrustructure.Migrations;
using SMSServiceInfrustructure.Model.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceImplementation.Services.Message
{
    public class GroupPhoneService : IGroupPhoneService
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IMapper _mapper;
        public GroupPhoneService(
            ApplicationDbContext dbContext,        
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ResponseMessage> AddGroupPhone(GroupPhonePostDto addGroupPhone)
        {

            string phoneNumber = addGroupPhone.PhoneNumber;

            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                
            }
            else if ((phoneNumber.StartsWith("9") || phoneNumber.StartsWith("7")) && phoneNumber.Length == 9)
            {
               
                phoneNumber = "0" + phoneNumber;
            }
            else if ( phoneNumber.StartsWith("251")&& phoneNumber.Substring(3).Length==9)
            {
                phoneNumber = "0" + phoneNumber.Substring(3);
            }
            else if (phoneNumber.StartsWith("+251")&& phoneNumber.Substring(4).Length==9)
            {
                phoneNumber = "0" + phoneNumber.Substring(4);
            }
            else
            {
                  return new ResponseMessage
                {
                      Data=new GroupPhonePostDto {

                          FullName= addGroupPhone.FullName,                          
                          PhoneNumber= addGroupPhone.PhoneNumber,
                         MessageGrpup = addGroupPhone.MessageGrpup,
                          ErrorMessage = "Phonenumber is Invalid",
                      },

                    Message = "Phonenumber is Invalid",
                    Success = false
                };
            }

           

                MessageGroupPhone groupPhone = new MessageGroupPhone
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    CreatedById = addGroupPhone.CreatedById,
                    FullName = addGroupPhone.FullName,
                    PhoneNumber = phoneNumber,
                    Remark = addGroupPhone.Remark,
                    MessageGroupId = addGroupPhone.MessageGroupId,
                    Rowstatus = RowStatus.ACTIVE,

                };

            try
            {
                await _dbContext.MessageGroupPhones.AddAsync(groupPhone);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Data = new GroupPhonePostDto
                    {

                        FullName = addGroupPhone.FullName,
                        PhoneNumber = addGroupPhone.PhoneNumber,
                        MessageGrpup = addGroupPhone.MessageGrpup,
                        ErrorMessage = "Dublicate phone number "
                    },

                    Message = "Dublicate phone number ",
                    Success = false
                };

            }
            



            return new ResponseMessage
            {

                Message = "Added Successfully",
                Success = true
            };
        }

        public async Task<ResponseMessage> UpdateGroupPhone(GroupPhoneGetDto addGroupPhone)
        {


            var GroupPhone = _dbContext.MessageGroupPhones.Find(addGroupPhone.Id);

            if (GroupPhone != null)
            {

                GroupPhone.FullName = addGroupPhone.FullName;
                GroupPhone.PhoneNumber = addGroupPhone.PhoneNumber;
                GroupPhone.Remark = addGroupPhone.Remark;
                GroupPhone.MessageGroupId = addGroupPhone.MessageGroupId;

                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {

                    Message = "Updated Successfully",
                    Success = true
                };

            }
            else
            {
                return new ResponseMessage
                {

                    Message = "No GroupPhone Found",
                    Success = false
                };
            }




        }

        public async Task<List<GroupPhoneGetDto>> GetGroupPhones(Guid MessageGroupId)
        {
            var GroupPhoneList = await _dbContext.MessageGroupPhones.Where(x=>x.MessageGroupId== MessageGroupId).AsNoTracking()
                                    .ProjectTo<GroupPhoneGetDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync();
            return GroupPhoneList;
        }
        public async Task<ResponseMessage> AddGroupPhoneFromExcel(IFormFile ExcelFile,string createdById)
        {
            try
            {

                List<GroupPhonePostDto> phoneNumbersErrors = new List<GroupPhonePostDto>();

                using (var package = new ExcelPackage(ExcelFile.OpenReadStream()))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Assuming the data starts from the second row
                    {
                        GroupPhonePostDto excelData = new GroupPhonePostDto();
                        var MessageGroup = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty;
                        var MessageGroupId = _dbContext.MessageGroups.Where(x => x.GroupCode == MessageGroup).FirstOrDefault();
                        if (MessageGroupId != null)
                        {
                            excelData.MessageGroupId = MessageGroupId.Id;
                            excelData.MessageGrpup = MessageGroup;
                            excelData.FullName = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
                            excelData.PhoneNumber = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;                            
                            excelData.Remark = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty;
                            excelData.CreatedById = createdById;


                           var result = await AddGroupPhone(excelData);

                            if (!result.Success)
                            {
                                phoneNumbersErrors.Add((GroupPhonePostDto)(result.Data));
                            }
                        }
                       
                    }
                }
                return new ResponseMessage
                {
                    Data=phoneNumbersErrors,
                    Message = "Add Successfully From Excel!!!",
                    Success = true
                };

            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {

                    Message = ex.InnerException.Message,
                    Success = false
                };
            }

        }
            
            

    }
}
