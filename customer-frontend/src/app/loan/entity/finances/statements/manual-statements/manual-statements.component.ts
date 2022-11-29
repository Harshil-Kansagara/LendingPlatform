import { Component, Input } from '@angular/core';
import { EntityFinanceAC } from '../../../../../utils/service';
import { ManualReportRow } from './models/manualReportRow.model';
import { ManualRowType } from './models/manualRowType.enum';
import { ToastrService } from 'ngx-toastr';
import { Constant } from '../../../../../shared/constant';
@Component({
  selector: 'app-manual-statements',
  templateUrl: './manual-statements.component.html',
  styleUrls: ['./manual-statements.component.scss']
})
export class ManualStatementsComponent {

  constructor(private readonly toastrService: ToastrService) { }
  @Input() data: ManualReportRow;
  @Input() uploadTitle;
  @Input() entityFinance: EntityFinanceAC;
  @Input() statementId: number;
  @Input() isReadOnly = false;
  @Input() showDeleteButton: boolean;
  @Input() showUploadButton = true;
  @Input() noFileslabel = false;
  selectedAll: boolean;

  /* Rendering value using for loop in ngModel causes undesired
** behaviour while changing the value in input so to fix that
** we use trackByFn to track by index instead of by identity
*/
  trackByFn(index: any) {
    return index;
  }


  isCollapsed = -1;
  isChildCollapsed = -1;
  collapsed(i: number) {
    if (this.isCollapsed !== i) {
      this.isCollapsed = i
    } else {
      this.isCollapsed = -1
    }
  }
  childCollapsed(i: number) {
    if (this.isChildCollapsed !== i) {
      this.isChildCollapsed = i
    } else {
      this.isChildCollapsed = -1
    }
  }
  /**
   * Add the child row.
   * @param row Parent ManualReportRow class object.
   */
  addChild(row: ManualReportRow, i: number): void {
    if (!row.Rows || row.Rows.length === 0) {
      this.emptyCells(row.Cells);
      this.isCollapsed = i;
    }
    this.addRow(row.Rows, ManualRowType.Child);
  }

  /**
 * Add the sub child row.
 * @param row Child ManualReportRow class object.
 */
  addSubChild(row: ManualReportRow, i: number): void {
    if (!row.Rows || row.Rows.length === 0) {
      this.emptyCells(row.Cells);
      this.isChildCollapsed = i;
    }
    this.addRow(row.Rows, ManualRowType.SubChild);
  }
  /**
   * Add the new row in the given rows.
   * @param rows list of existing rows in the list.
   * @param rowType Specify the rowtype like Child or subchild.
   */
  addRow(rows: Array<ManualReportRow>, rowType: ManualRowType): void {
    if (!rows) {
      rows = new Array<ManualReportRow>();
    }

    const manualReportRow = new ManualReportRow();
    manualReportRow.Id = this.getGuid();
    manualReportRow.RowType = rowType;
    manualReportRow.Cells = new Array<string>();
    const totalYears = this.entityFinance.entityFinanceYearlyMappings.length;
    for (let i = 0; i < totalYears; i++) {
      manualReportRow.Cells.push("");
    }
    rows.push(manualReportRow);
  }
  /**
   * It returns true when user can change the amount else only view the amount.
   * @param row ManualReportRow class object.
   */
  isEditAmount(row: ManualReportRow): boolean {
    return !(row.Rows && row.Rows.length);
  }

  /**
   * Delete the selected rows.
   * @param rows List of rows.
   */
  deleteRows(rows: ManualReportRow[]): void {

    const parentRows = rows.filter(s => s.RowType === ManualRowType.Parent);
    for (const parentRow of parentRows) {
      if (parentRow.IsSelected) {
        parentRow.Rows = [];
        this.emptyCells(parentRow.Cells, false);
      }
      else if (parentRow.Rows && parentRow.Rows.length > 0) {
        // Remove selected childs.
        parentRow.Rows = parentRow.Rows.filter(s => !s.IsSelected);

        for (const childRow of parentRow.Rows) {
          if (childRow.Rows && childRow.Rows.length > 0) {
            // Remove selected sub childs.
            childRow.Rows = childRow.Rows.filter(s => !s.IsSelected);
            // Update the child row amount.
            this.updateRowCellsAmount(childRow);
          }
        }
        // Update the parent row amount.
        this.updateRowCellsAmount(parentRow);
      }
    }
  }
  /**
   * Update the row cells amount by the following child rows.
   * @param parentRow Parent or child row object.
   */
  updateRowCellsAmount(parentRow: ManualReportRow): void {
    for (const index in parentRow.Cells) {
      this.updateParentRowsAmount(parentRow, Number(index));
    }
  }
  /**
   * Remove cells value.
   * @param cells list of cells.
   */
  emptyCells(cells: Array<string>, isAddDefaultValue = true) {
    for (const index in cells) {
      cells[index] = isAddDefaultValue ? "0.00" : "";
    }
  }
  /**
   * Update child and parent rows amount of the specific year.
   * @param childRow Child row object
   * @param index Index of the cells.
   * @param parentRow Parent row object.
   */
  updateChildAndParentRowsAmount(childRow: ManualReportRow, index: number, parentRow: ManualReportRow) {
    // Update child rows amount.
    this.updateParentRowsAmount(childRow, index);
    // Update parent rows amount.
    this.updateParentRowsAmount(parentRow, index);
  }
  /**
   * Update child and parent rows amount.
   * @param parentRow Parent row object.
   * @param index Index of the cells.
   */
  updateParentRowsAmount(parentRow: ManualReportRow, index: number) {
    let totalAmount = 0;
    parentRow.Rows.forEach(s => totalAmount += Number(s.Cells[index]));
    parentRow.Cells[index] = totalAmount.toFixed(2).toString();
  }

  /**
   * Check the title in the currentlist.
   * @param rows
   * @param title
   */
  checkDuplicateTitleInRows(rows: ManualReportRow[], title: string): boolean {
    // If empty title then display error.
    if (!title) {
      this.toastrService.error(Constant.requiredTitle);
      return false;
    }

    // Check duplicate title.
    const titleCount = rows.filter(d => d.Title?.toLowerCase() === title.toLowerCase()).length;
    if (titleCount > 1) {
      this.toastrService.error(`${title} account appears more then once. It must be unique`);
      return false;
    }
    return true;
  }
  /**
   * Generate new guid. */
  getGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  /**
   * Modify all state of the rows based on the state parameter.
   * @param state Pass true or false based on the checkbox select.
   */
  checkAll(state: boolean): void {
    const parentRows = this.data.Rows.filter(s => s.RowType === ManualRowType.Parent);
    this.updateSelectedCheckboxOnRows(parentRows, state);
  }

  /**
   * Modify all state of the rows based on the state parameter.
   * @param parentRows all Rows.
   * @param state Pass true or false based on the checkbox select.
   */
  updateSelectedCheckboxOnRows(parentRows: ManualReportRow[], state: boolean): void {
    for (const parentRow of parentRows) {
      parentRow.IsSelected = state;
      if (parentRow.Rows) {
        // Update in the child rows.
        this.updateSelectedCheckboxOnRows(parentRow.Rows, state);
      }
    }
  }
}
