using System;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.Services
{
    /// <summary>
    /// Concrete factory implementation for creating gym management entities
    /// Implements Factory Design Pattern
    /// </summary>
    public class EntityFactory : IEntityFactory
    {
        #region Entity Creation Methods

        /// <summary>
        /// Creates a new Member entity with validation
        /// </summary>
        public Member CreateMember(string memberId, string fullName, string contactNumber,
            string trainerName = null, string subscriptionType = null, string medicalHistory = null)
        {
            if (string.IsNullOrWhiteSpace(memberId))
                throw new ArgumentException("Member ID cannot be empty", nameof(memberId));
            
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty", nameof(fullName));
            
            if (string.IsNullOrWhiteSpace(contactNumber))
                throw new ArgumentException("Contact number cannot be empty", nameof(contactNumber));

            return new Member
            {
                MemberId = memberId,
                FullName = fullName,
                ContactNumber = contactNumber,
                TrainerName = trainerName,
                SubscriptionType = subscriptionType,
                JoinDate = DateTime.Now.ToString("yyyy-MM-dd"),
                MedicalHistory = medicalHistory
            };
        }

        /// <summary>
        /// Creates a new Trainer entity with validation
        /// </summary>
        public Trainer CreateTrainer(string trainerId, string fullName, string contactNumber,
            string specialty = null, string experience = null, string email = null)
        {
            if (string.IsNullOrWhiteSpace(trainerId))
                throw new ArgumentException("Trainer ID cannot be empty", nameof(trainerId));
            
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty", nameof(fullName));
            
            if (string.IsNullOrWhiteSpace(contactNumber))
                throw new ArgumentException("Contact number cannot be empty", nameof(contactNumber));

            return new Trainer
            {
                TrainerId = trainerId,
                FullName = fullName,
                ContactNumber = contactNumber,
                Specialty = specialty,
                Experience = experience,
                Email = email,
                JoinDate = DateTime.Now.ToString("yyyy-MM-dd")
            };
        }

        /// <summary>
        /// Creates a new Equipment entity with validation
        /// </summary>
        public Equipment CreateEquipment(string equipmentId, string name, int quantity,
            string condition = null, string category = null)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("Equipment ID cannot be empty", nameof(equipmentId));
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Equipment name cannot be empty", nameof(name));
            
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            return new Equipment
            {
                EquipmentId = equipmentId,
                Name = name,
                Quantity = quantity,
                Condition = condition,
                Category = category
            };
        }

        #endregion

        #region Database Insertion Methods

        /// <summary>
        /// Validates and inserts a Member into the database
        /// </summary>
        public bool InsertMember(Member member, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                using var transaction = conn.BeginTransaction();
                try
                {
                    // Check if member ID already exists
                    var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Members WHERE MemberId = @id", conn, transaction);
                    checkCmd.Parameters.AddWithValue("@id", member.MemberId);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Member ID already exists";
                        transaction.Rollback();
                        return false;
                    }

                    // Insert member
                    var cmd = new SqliteCommand(@"
                        INSERT INTO Members (MemberId, FullName, TrainerName, JoinDate, SubscriptionType, ContactNumber, MedicalHistory) 
                        VALUES (@id, @name, @trainer, @date, @type, @contact, @medical)", conn, transaction);

                    cmd.Parameters.AddWithValue("@id", member.MemberId);
                    cmd.Parameters.AddWithValue("@name", member.FullName);
                    cmd.Parameters.AddWithValue("@trainer", member.TrainerName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@date", member.JoinDate);
                    cmd.Parameters.AddWithValue("@type", member.SubscriptionType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@contact", member.ContactNumber);
                    cmd.Parameters.AddWithValue("@medical", member.MedicalHistory ?? (object)DBNull.Value);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        errorMessage = "Failed to insert member - no rows affected";
                        transaction.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Validates and inserts a Trainer into the database
        /// </summary>
        public bool InsertTrainer(Trainer trainer, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                using var transaction = conn.BeginTransaction();
                try
                {
                    // Check if trainer ID already exists
                    var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Trainers WHERE TrainerId = @trainerId", conn, transaction);
                    checkCmd.Parameters.AddWithValue("@trainerId", trainer.TrainerId);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Trainer ID already exists";
                        transaction.Rollback();
                        return false;
                    }

                    // Insert trainer
                    var cmd = new SqliteCommand(@"
                        INSERT INTO Trainers (TrainerId, FullName, ContactNumber, Specialty, Experience, Email, JoinDate) 
                        VALUES (@trainerId, @fullName, @contact, @specialty, @experience, @email, @joinDate)", conn, transaction);

                    cmd.Parameters.AddWithValue("@trainerId", trainer.TrainerId);
                    cmd.Parameters.AddWithValue("@fullName", trainer.FullName);
                    cmd.Parameters.AddWithValue("@contact", trainer.ContactNumber);
                    cmd.Parameters.AddWithValue("@specialty", trainer.Specialty ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@experience", trainer.Experience ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", trainer.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@joinDate", trainer.JoinDate);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        errorMessage = "Failed to insert trainer - no rows affected";
                        transaction.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Validates and inserts Equipment into the database
        /// </summary>
        public bool InsertEquipment(Equipment equipment, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                using var transaction = conn.BeginTransaction();
                try
                {
                    // Check if equipment with same name already exists (optional warning, not blocking)
                    var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Equipment WHERE Name = @name", conn, transaction);
                    checkCmd.Parameters.AddWithValue("@name", equipment.Name);
                    long exists = (long)checkCmd.ExecuteScalar();

                    // Insert equipment
                    var cmd = new SqliteCommand(@"
                        INSERT INTO Equipment (EquipmentId, Name, Quantity, Condition, Category) 
                        VALUES (@id, @name, @qty, @cond, @category)", conn, transaction);

                    cmd.Parameters.AddWithValue("@id", equipment.EquipmentId);
                    cmd.Parameters.AddWithValue("@name", equipment.Name);
                    cmd.Parameters.AddWithValue("@qty", equipment.Quantity);
                    cmd.Parameters.AddWithValue("@cond", equipment.Condition ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@category", equipment.Category ?? (object)DBNull.Value);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        errorMessage = "Failed to insert equipment - no rows affected";
                        transaction.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        #endregion
    }
}
