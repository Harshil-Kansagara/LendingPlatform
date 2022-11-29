import { ManualRowType } from './manualRowType.enum';

export class ManualReportRow {
  RowType: ManualRowType;
  Id: string;
  Title: string;
  Cells: Array<string>;
  Rows: Array<ManualReportRow>;
  IsSelected: boolean;
  constructor() {
    this.Rows = new Array<ManualReportRow>();
    this.Cells = new Array<string>();
  }
}
