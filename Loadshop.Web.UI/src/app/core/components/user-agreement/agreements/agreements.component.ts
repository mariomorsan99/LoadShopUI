import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'kbxl-agreements',
  templateUrl: './agreements.component.html',
  styleUrls: ['./agreements.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AgreementsComponent implements OnInit, OnDestroy {
  destroyed$ = new Subject<boolean>();
  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
    this.route.params
      .pipe(
        map(params => params.documentType),
        takeUntil(this.destroyed$)
      )
      .subscribe((documentType: string) => {
        if (documentType && documentType.toLowerCase() === 'privacy') {
          this.scrollToTarget('privacy');
        } else {
          this.scrollToTarget('terms');
        }
      });
  }
  ngOnDestroy(): void {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }
  scrollToTarget(id: string): void {
    const element = document.getElementById(id);

    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }
}
