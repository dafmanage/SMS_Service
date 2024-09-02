using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedInfrustructure.Data
{
    public class EnumList
    {

       
        public enum RowStatus
        {
            ACTIVE,
            INACTIVE
        }

        public enum Gender
        {
            MALE,
            FEMALE
        }

        public enum MaritalStatus
        {
            SINGLE,
            MARRIED,
            DIVORCE,
            WIDOW
        }


        public enum EmploymentType
        {
            PERMANENT,
            CONTRAT
        }

        public enum EmploymentStatus
        {
            ACTIVE,
            TERMINATED,
            RESIGNED
        }
        public enum OrganizationStatus
        {
            ACTIVE,
            INACTIVE,
            TERMINATED
        }
        public enum EmploymentPosition
        {
            DEPUTY_MANAGER,
            HRM,
            FINANCE,
            MARKETING,
            DEVELOPER
        }

        public enum PaymentType
        {
            PERDAY,
            PERWEEK,
            PERMONTH
        }

        public enum GeneralCodeType
        {
            EMPLOYEEPREFIX,
            TASKPREFIX
        }

        public enum ApplicantStatus
        {
            APPLIED,
            SEEN,
            PROGRESS,
            INTERVIEW,
            REJECTED,
            HIRED,
            BLACKLISTED
        }

        public enum VacancyType
        {
            INTERNAL,
            EXTERNAL,
            BOTH
        }

        public enum LeaveCategory
        {
            ANNUAL,
            UNPAID,
            OTHER
        }
        public enum LeaveRequestStatus
        {
            PENDING,
            APPROVED,
            REJECTED
        }

        public enum FamilyRelation
        {
           PARENT,
           SPOUSE,
           CHILD
        }
        public enum GeneralHrmSetting
        {
            PROBATIONPERIOD,
            ANNUALLEAVESTARTMONTH,
            PERFORMANCESTARTDATE,
            PERFORMANCEENDDATE,
        }

        public enum ProjectStatus
        {
            PENDING,
            ONGOING,
            COMPLETED,
            CANCELD
        }

        public enum AssignedTo
        {
            TEAM,
            EMPLOYEE
        }

        public enum TaskStatuses
        {
            NOTSTARTED,
            INPROGRESS,
            COMPLETE
            
        }

        public enum TaskPriority
        { 
            LOW,
            MEDIUM,
            HIGH 
        }   
        public enum TaskApproval
        {
            PENDING,
            APPROVED,
            REJECTED
        }
        public enum ComplaintType
        {

        }
        public enum ComplaintStatus
        {
            PENDING,
            INPROGRESS,
            ONHOLD,
            RESOLVED,
            REOPENED
        }

        public enum MessageStatus
        {
            SENT,
            UNSENT
        }

        public enum MessageLanguage
        {
            AMHARIC,
            ENGLISH 
        }
    }
}
