using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Enums
{
    public enum BrokerEvent
    {
        ExpenseReceived,
        ExpenseLabelUpdated,
        ExpensePriceUpdated,
        ExpenseRemoved,
        ExpenseAttachmentRemoved,
        InvoiceGenerated,
        InvoiceRemoved,
        InvoiceRestored,
        DossierCreated,
        ExpenseAddedToDossier,
        ExpenseRemovedFromDossier,
        InvoiceAddedToDossier,
        InvoiceRemovedFromDossier,
        DossierClosed,
        DossierDeleted,
        DossierUpdated,
        DossierRecallForModification
    }
}
