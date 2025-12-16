import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Garages } from './garages';

describe('Garages', () => {
  let component: Garages;
  let fixture: ComponentFixture<Garages>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Garages]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Garages);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
