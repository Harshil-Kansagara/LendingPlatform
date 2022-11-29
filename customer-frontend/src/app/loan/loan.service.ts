import { Injectable } from '@angular/core';
import { Constant } from '../shared/constant';
import { AppService } from '../services/app.service';
import { Options } from '@m0t0r/ngx-slider';
import { LoanPurposeAC } from '../utils/serviceNew';

@Injectable({
  providedIn: 'root'
})

export class LoanService {

  constructor(private readonly appService: AppService) { }

  /**
   * Method to calculate the monthly EMI.
   * @param principleAmount Principal amount
   * @param yearlyInterestRate Yearly interest rate
   * @param tenureYears Tenure in years
   */
  calculateMonthlyEMI(principleAmount: number, yearlyInterestRate: number, tenureYears: number): number {
    if (principleAmount !== 0 && tenureYears !== 0) {
      const monthCount = 12;
      const hundred = 100;
      const two = 2;
      const monthlyInterestRate = yearlyInterestRate / (monthCount * hundred);
      return parseFloat((Math.round((((principleAmount * monthlyInterestRate * (Math.pow(1 + monthlyInterestRate, tenureYears * monthCount)))
        / (Math.pow(1 + monthlyInterestRate, tenureYears * monthCount) - 1)) + Number.EPSILON) * hundred) / hundred).toFixed(two));
    }
    return 0.00;
  }

  /**
   * Method to get the interest rate for given credit score.
   * @param selfDeclaredCreditScore
   */
  async getRateOfInterestForGivenCreditScore(selfDeclaredCreditScore: string): Promise<number> {
    const appSettings = await this.appService.getAppSettings();

    if (selfDeclaredCreditScore === Constant.excellent) {
      return Number((await this.appService.getAppSettings()).filter(x => x.fieldName === 'LoanNeeds:InterestRateForExcellentCreditScore')[0].value);
    } else if (selfDeclaredCreditScore === Constant.good) {
      return Number(appSettings.filter(x => x.fieldName === 'LoanNeeds:InterestRateForGoodCreditScore')[0].value);
    } else if (selfDeclaredCreditScore === Constant.fair) {
      return Number(appSettings.filter(x => x.fieldName === 'LoanNeeds:InterestRateForFairCreditScore')[0].value);
    } else if (selfDeclaredCreditScore === Constant.average) {
      return Number(appSettings.filter(x => x.fieldName === 'LoanNeeds:InterestRateForAverageCreditScore')[0].value);
    } else {
      return Number(appSettings.filter(x => x.fieldName === 'LoanNeeds:InterestRateForPoorCreditScore')[0].value);
    }
  }

  /**
   * Method returns Options objects for amount and period slider.
   * @param currencySymbol Currency symbol
   * @param periodUnit Period unit
   * @param loanPurpose Loan purpose
   * @param yearlyInterestRateForEMI Yearly interest rate for EMI
   */
  getAmountAndPeriodOptionsSet(currencySymbol: string, periodUnit: string, loanPurpose: LoanPurposeAC, yearlyInterestRateForEMI: number): Options[] {

    const rangeTypeLoanAmount = loanPurpose.loanPurposeRangeTypeMappings.filter(x => x.rangeTypeName === Constant.rangeTypeLoanAmount)[0];
    const rangeTypeLoanPeriod = loanPurpose.loanPurposeRangeTypeMappings.filter(x => x.rangeTypeName === Constant.rangeTypeLoanPeriod)[0];
    const two = 2;
    const amountOptions: Options = {
      animate: false,
      floor: (Number)((rangeTypeLoanAmount.minimum).toFixed(two)),
      ceil: (Number)((rangeTypeLoanAmount.maximum).toFixed(two)),
      showSelectionBar: true,
      step: rangeTypeLoanAmount.stepperAmount,
      translate: (value: number): string => (`${currencySymbol} ${value.toLocaleString('en')}`)
    };
    const periodOptions: Options = {
      animate: false,
      floor: (Number)((rangeTypeLoanPeriod.minimum).toFixed(two)),
      ceil: (Number)((rangeTypeLoanPeriod.maximum).toFixed(two)),
      step: rangeTypeLoanPeriod.stepperAmount,
      showSelectionBar: true,
      translate: (value: number): string => value > 1 ? (`${value.toLocaleString('en')} ${periodUnit}s`) :
        (`${value.toLocaleString('en')} ${periodUnit}`)
    };
    return [amountOptions, periodOptions];
  }

  /**
   * Method is used to get the period in month format on basis of monthly payment
   * @param amount Loan amount
   * @param monthlyPayment Monthly Payment
   * @param yearlyRate Year rate value which can be fixed rate, variable rate or annual percentage rate
   */
  calculatePeriod(amount: number, monthlyPayment: number, yearlyRate: number): number {
    const months = 12;
    const two = 2;
    const hundreds = 100;
    const monthlyRate = yearlyRate / (months * hundreds);
    const nper1 = Math.log(1 - ((amount / monthlyPayment) * (monthlyRate)));
    const nper2 = Math.log(1 + monthlyRate);
    const nper = -Math.round(nper1 / nper2);
    return (Number)((nper / months).toFixed(two));
  }

  /**
   * Method to change the min and max values of EMI slider.
   * @param amount Current selected amount
   * @param yearlyInterestRateForEMI Yearly interest rate for EMI
   * @param minPeriod Minimum value of period
   * @param maxPeriod Maximum value of period
   * @param periodStep Step size of period slider
   * @param currencySymbol Currency symbol
   */
  changeMinMaxValuesOfMonthlyPayment(amount: number, yearlyInterestRateForEMI: number, minPeriod: number,
    maxPeriod: number, periodStep: number, currencySymbol: string) : Options {
    const two = 2;
    const stepArray = [];
    for (let i = maxPeriod; i >= minPeriod; i = i - periodStep) {
      stepArray.push({ value: (Number)(this.calculateMonthlyEMI(amount, yearlyInterestRateForEMI, i).toFixed(two)) });
    }
    return {
      animate: false,
      stepsArray: stepArray,
      showSelectionBar: true,
      translate: (value: number): string => (`${currencySymbol} ${value.toLocaleString('en')}`),
      showTicks: true,
    };
  }
}
