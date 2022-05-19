using DropboxSync.BLL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public interface IUploadService : IServiceBase<UploadEntity, Guid>
    {
        public UploadEntity? GetByUploadId(string uploadId);

        /// <summary>
        /// Retrieve all the uploads linked to an Expense
        /// </summary>
        /// <param name="expenseId">The ID of the expense as a <see cref="Guid"/></param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="UploadEntity"/></returns>
        public IEnumerable<UploadEntity>? GetExpenseRelatedUploads(Guid expenseId);

        /// <summary>
        /// Retrieve the upload linked to an invoice
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice as a <see cref="Guid"/></param>
        /// <returns>A object of type <see cref="UploadEntity"/></returns>
        public UploadEntity? GetInvoiceRelatedUpload(Guid invoiceId);
    }
}
