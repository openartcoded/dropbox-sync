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
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="UploadEntity"/></returns>
        public IEnumerable<UploadEntity>? GetExpenseRelatedUploads(Guid expenseId);

        /// <summary>
        /// Retrieve the upload linked to an invoice
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice as a <see cref="Guid"/></param>
        /// <returns>An object of type <see cref="UploadEntity"/> if any upload exist in database. <c>null</c> Otherwise.</returns>
        public UploadEntity? GetInvoiceRelatedUpload(Guid invoiceId);

        /// <summary>
        /// Retrieve the Upload linked to a document
        /// </summary>
        /// <param name="documentId">The ID of the document as a <see cref="Guid"/></param>
        /// <returns>An object of type <see cref="UploadEntity"/> if any upload exist in database. <c>null</c> Otherwise.</returns>
        public UploadEntity? GetDocumentRelatedUpload(Guid documentId);
    }
}
