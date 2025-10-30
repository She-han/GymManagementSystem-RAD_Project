using System;
using GymManagementSystem.Models;

namespace GymManagementSystem.Services
{
    /// <summary>
    /// Factory interface for creating gym management entities
    /// </summary>
    public interface IEntityFactory
    {
        /// <summary>
        /// Creates a new Member entity
        /// </summary>
        Member CreateMember(string memberId, string fullName, string contactNumber, 
            string trainerName = null, string subscriptionType = null, string medicalHistory = null);

        /// <summary>
        /// Creates a new Trainer entity
        /// </summary>
        Trainer CreateTrainer(string trainerId, string fullName, string contactNumber, 
            string specialty = null, string experience = null, string email = null);

        /// <summary>
        /// Creates a new Equipment entity
        /// </summary>
        Equipment CreateEquipment(string equipmentId, string name, int quantity, 
            string condition = null, string category = null);

        /// <summary>
        /// Validates and inserts a Member into the database
        /// </summary>
        bool InsertMember(Member member, out string errorMessage);

        /// <summary>
        /// Validates and inserts a Trainer into the database
        /// </summary>
        bool InsertTrainer(Trainer trainer, out string errorMessage);

        /// <summary>
        /// Validates and inserts Equipment into the database
        /// </summary>
        bool InsertEquipment(Equipment equipment, out string errorMessage);
    }
}
